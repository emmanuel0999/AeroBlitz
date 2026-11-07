#if UNITY_EDITOR
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEditor.Media;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AeroBlitz
{
    /// <summary>
    /// Ayudas para grabar el video promocional desde el Editor (no existen en el juego publicado).
    /// Se controlan desde la ventana "Aero Blitz > Grabar tráiler".
    /// </summary>
    public static class TrailerCheats
    {
        /// <summary>Modo tráiler activo: no se guarda la partida ni se envían puntuaciones al ranking.</summary>
        public static bool Active;
        public static bool Invincible;
        public static bool FrequentPowerUps;
        public static bool ForceTrial;
    }

    /// <summary>
    /// Graba dos videos a la vez a 30 fps: el vertical tal cual (TikTok/Shorts) y otro horizontal
    /// 1920x1080 con el juego en el centro sobre un rótulo (ficha de Google Play / YouTube).
    /// </summary>
    public class TrailerRecorder : MonoBehaviour
    {
        public const int Fps = 30;
        const int MaxHeight = 1920;
        public static TrailerRecorder Instance { get; private set; }
        public bool Recording { get; private set; }
        public string CurrentPath { get; private set; }
        public int Frames { get; private set; }
        /// <summary>Tamaño real del video vertical (0x0 hasta el primer fotograma).</summary>
        public Vector2Int Size => new Vector2Int(_w, _h);

        MediaEncoder _vertical, _wide;
        TrailerCompositor _compositor;
        Texture2D _scaled;
        RenderTexture _scaleRT;
        string _background;
        int _w, _h, _prevVSync, _prevTarget;
        readonly Stopwatch _clock = new Stopwatch();

        public static TrailerRecorder Ensure()
        {
            if (Instance == null)
            {
                var go = new GameObject("TrailerRecorder");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<TrailerRecorder>();
            }
            return Instance;
        }

        public static MediaEncoder CreateEncoder(string path, int width, int height)
        {
            var video = new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps),
                width = (uint)width,
                height = (uint)height,
                includeAlpha = false,
                bitRateMode = UnityEditor.VideoBitrateMode.High
            };
            return new MediaEncoder(path, video);
        }

        /// <summary>Crea basePath_vertical.mp4 y basePath_horizontal.mp4.</summary>
        public void StartRecording(string basePath, string backgroundPng)
        {
            if (Recording) return;
            CurrentPath = basePath;
            _background = backgroundPng;
            _w = _h = 0;
            Frames = 0;
            _prevVSync = QualitySettings.vSyncCount;
            _prevTarget = Application.targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Fps;
            Time.captureFramerate = Fps; // tiempo de juego fijo: el video sale fluido aunque el PC vaya lento
            Recording = true;
            _clock.Restart();
            StartCoroutine(CaptureLoop());
        }

        public void StopRecording()
        {
            if (!Recording) return;
            Recording = false;
            Time.captureFramerate = 0;
            QualitySettings.vSyncCount = _prevVSync;
            Application.targetFrameRate = _prevTarget;
            if (_vertical == null) return;
            _vertical.Dispose();
            _wide.Dispose();
            _vertical = _wide = null;
            _compositor.Dispose();
            _compositor = null;
            if (_scaleRT != null) { _scaleRT.Release(); Destroy(_scaleRT); Destroy(_scaled); _scaleRT = null; _scaled = null; }
            Debug.Log($"[Tráiler] Grabación guardada: {CurrentPath}_vertical.mp4 y _horizontal.mp4 ({_w}x{_h}, {Frames / (float)Fps:0.0} s)");
        }

        // Los encoders se crean con el tamaño real de la imagen capturada: en el Editor Screen.width
        // puede no coincidir con lo que entrega la captura (pasa al reducir el Scale de la pestaña Game).
        void Open(int sw, int sh)
        {
            float k = Mathf.Min(1f, MaxHeight / (float)sh);
            _w = Mathf.RoundToInt(sw * k) & ~1; // el códec H.264 necesita medidas pares
            _h = Mathf.RoundToInt(sh * k) & ~1;
            _vertical = CreateEncoder(CurrentPath + "_vertical.mp4", _w, _h);
            _wide = CreateEncoder(CurrentPath + "_horizontal.mp4", TrailerCompositor.WideW, TrailerCompositor.WideH);
            _compositor = new TrailerCompositor(_w, _h, _background);
            Debug.Log($"[Tráiler] Grabando a {_w}x{_h} (captura {sw}x{sh}, pantalla {Screen.width}x{Screen.height})");
        }

        /// <summary>Lleva la captura al tamaño del video; si ya coincide se usa tal cual.</summary>
        Texture2D ToFrame(Texture2D shot)
        {
            if (shot.width == _w && shot.height == _h && shot.format == TextureFormat.RGBA32) return shot;
            if (_scaled == null)
            {
                _scaled = new Texture2D(_w, _h, TextureFormat.RGBA32, false);
                _scaleRT = new RenderTexture(_w, _h, 0, RenderTextureFormat.ARGB32);
                _scaleRT.Create();
            }
            var prev = RenderTexture.active;
            RenderTexture.active = _scaleRT;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _w, _h, 0);
            Graphics.DrawTexture(new Rect(0, 0, _w, _h), shot);
            GL.PopMatrix();
            _scaled.ReadPixels(new Rect(0, 0, _w, _h), 0, 0, false);
            _scaled.Apply(false);
            RenderTexture.active = prev;
            return _scaled;
        }

        IEnumerator CaptureLoop()
        {
            var wait = new WaitForEndOfFrame();
            while (Recording)
            {
                yield return wait;
                if (!Recording) break;
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                if (_vertical == null) Open(shot.width, shot.height);
                var frame = ToFrame(shot);
                _vertical.AddFrame(frame);
                _wide.AddFrame(_compositor.Compose(frame));
                Frames++;
                Destroy(shot);

                // Con PC rápido el Editor iría a más de 30 fps y el juego correría acelerado mientras grabas
                double rest = 1.0 / Fps - _clock.Elapsed.TotalSeconds;
                if (rest > 0.001) System.Threading.Thread.Sleep((int)(rest * 1000));
                _clock.Restart();
            }
        }

        void OnDisable() => StopRecording(); // al salir de Play el archivo queda cerrado y válido
    }

    /// <summary>Coloca la imagen vertical del juego en el centro de un lienzo 1920x1080 con rótulo de fondo.</summary>
    public sealed class TrailerCompositor : System.IDisposable
    {
        public const int WideW = 1920, WideH = 1080;
        public readonly Texture2D Frame;
        readonly Texture2D _background;
        readonly RenderTexture _mip, _target;
        readonly Rect _gameRect;

        public TrailerCompositor(int gameW, int gameH, string backgroundPng)
        {
            if (!string.IsNullOrEmpty(backgroundPng) && File.Exists(backgroundPng))
            {
                _background = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                _background.LoadImage(File.ReadAllBytes(backgroundPng));
            }
            // Con mipmaps la reducción a 1080 de alto se ve nítida y sin dientes de sierra
            _mip = new RenderTexture(gameW, gameH, 0, RenderTextureFormat.ARGB32)
            { useMipMap = true, autoGenerateMips = true, filterMode = FilterMode.Trilinear };
            _mip.Create();
            _target = new RenderTexture(WideW, WideH, 0, RenderTextureFormat.ARGB32);
            _target.Create();
            Frame = new Texture2D(WideW, WideH, TextureFormat.RGBA32, false);
            int gw = Mathf.RoundToInt(WideH * gameW / (float)gameH) & ~1;
            _gameRect = new Rect((WideW - gw) / 2, 0, gw, WideH);
        }

        public Texture2D Compose(Texture2D shot)
        {
            Graphics.Blit(shot, _mip);
            var prev = RenderTexture.active;
            RenderTexture.active = _target;
            GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, WideW, WideH, 0);
            if (_background != null) Graphics.DrawTexture(new Rect(0, 0, WideW, WideH), _background);
            Graphics.DrawTexture(_gameRect, _mip);
            GL.PopMatrix();
            Frame.ReadPixels(new Rect(0, 0, WideW, WideH), 0, 0, false);
            Frame.Apply(false);
            RenderTexture.active = prev;
            return Frame;
        }

        public void Dispose()
        {
            if (_background != null) Object.Destroy(_background);
            _mip.Release(); Object.Destroy(_mip);
            _target.Release(); Object.Destroy(_target);
            Object.Destroy(Frame);
        }
    }
}
#endif
