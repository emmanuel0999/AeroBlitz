using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Ventana para grabar el video promocional: elige nivel, nave y armas, activa ayudas
    /// y graba clips MP4 en la carpeta TrailerCaptures. Solo funciona en modo Play.
    /// </summary>
    public class TrailerWindow : EditorWindow
    {
        const string Folder = "TrailerCaptures";
        const string CaptionFolder = Folder + "/rotulos";
        static readonly string[] CannonNames = { "Normal", "Doble", "Triple", "Plasma" };
        static readonly string[] CannonIds = { null, "eq_twin", "eq_triple", "eq_plasma" };

        const string KeyInvincible = "AeroBlitz.Trailer.Invincible";
        const string KeyPowerUps = "AeroBlitz.Trailer.PowerUps";
        const string KeyTrial = "AeroBlitz.Trailer.Trial";

        [SerializeField] int _level = 45;
        [SerializeField] int _ship;
        [SerializeField] int _cannon = 3;
        [SerializeField] bool _missiles = true;
        [SerializeField] bool _shield;
        [SerializeField] int _caption;
        Vector2 _scroll;

        [MenuItem("Aero Blitz/Grabar tráiler")]
        static void Open() => GetWindow<TrailerWindow>("Tráiler").minSize = new Vector2(340, 520);

        void OnInspectorUpdate() => Repaint();

        // Las ayudas son estáticas y se borran cada vez que Unity recarga los scripts (al pulsar Play):
        // se guardan en la sesión del Editor y se recuperan después de cada recarga
        [InitializeOnLoadMethod]
        static void Init()
        {
            TrailerCheats.Invincible = SessionState.GetBool(KeyInvincible, false);
            TrailerCheats.FrequentPowerUps = SessionState.GetBool(KeyPowerUps, false);
            TrailerCheats.ForceTrial = SessionState.GetBool(KeyTrial, false);
            // Al salir de Play el juego vuelve a guardar normalmente aunque no se recargue el dominio
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.EnteredEditMode) TrailerCheats.Active = false;
            };
        }

        static bool CheatToggle(string label, string key, bool value)
        {
            bool v = EditorGUILayout.Toggle(label, value);
            if (v != value)
            {
                SessionState.SetBool(key, v);
                // Si la casilla se queda con el teclado, la tecla Espacio (disparar) la desmarca
                GUIUtility.keyboardControl = 0;
            }
            return v;
        }

        /// <summary>Pasa el teclado a la pestaña Game para jugar sin tener que hacer clic en ella.</summary>
        static void FocusGame()
        {
            GUIUtility.keyboardControl = 0;
            EditorApplication.delayCall += () =>
            {
                var gameView = Type.GetType("UnityEditor.GameView,UnityEditor");
                if (gameView != null) FocusWindowIfItsOpen(gameView);
            };
        }

        static string[] Captions()
        {
            if (!Directory.Exists(CaptionFolder)) return new string[0];
            var files = Directory.GetFiles(CaptionFolder, "rotulo_*.png");
            Array.Sort(files);
            return files;
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.HelpBox("1) Abre la escena Main y pulsa Play.\n2) En la pestaña Game elige la resolución 1080x1920 (añádela con el botón + si no existe).\n3) Configura aquí, pulsa 'Jugar nivel' y luego 'Grabar'.", MessageType.Info);

            var ships = ShipCatalog.All;
            var shipNames = new string[ships.Count];
            for (int i = 0; i < ships.Count; i++) shipNames[i] = ships[i].id;

            EditorGUILayout.LabelField("Partida", EditorStyles.boldLabel);
            _level = EditorGUILayout.IntSlider("Nivel", _level, 1, GameConfig.TotalLevels);
            EditorGUILayout.LabelField("Mundo", WorldCatalog.ForLevel(_level).Name);
            if (_level < GameConfig.PickupsFromLevel) EditorGUILayout.LabelField(" ", $"Monedas y mejoras: desde el nivel {GameConfig.PickupsFromLevel}");
            _ship = EditorGUILayout.Popup("Nave", _ship, shipNames);
            _cannon = EditorGUILayout.Popup("Cañón", _cannon, CannonNames);
            _missiles = EditorGUILayout.Toggle("Misiles", _missiles);
            _shield = EditorGUILayout.Toggle("Escudo de Plasma", _shield);

            bool playing = Application.isPlaying && GameManager.Instance != null;
            using (new EditorGUI.DisabledScope(!playing))
            {
                int lang = Array.IndexOf(L.All, L.Current);
                var langNames = new string[L.All.Length];
                for (int i = 0; i < L.All.Length; i++) langNames[i] = L.NativeName(L.All[i]);
                int newLang = EditorGUILayout.Popup("Idioma del juego", lang, langNames);
                // Igual que en Ajustes: se reconstruye la interfaz con el idioma nuevo
                if (playing && newLang != lang) GameManager.Instance.SetLanguage(L.All[newLang]);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ayudas para grabar", EditorStyles.boldLabel);
            TrailerCheats.Invincible = CheatToggle("Invencible", KeyInvincible, TrailerCheats.Invincible);
            TrailerCheats.FrequentPowerUps = CheatToggle("Mejoras frecuentes", KeyPowerUps, TrailerCheats.FrequentPowerUps);
            TrailerCheats.ForceTrial = CheatToggle("Forzar prueba de arma", KeyTrial, TrailerCheats.ForceTrial);
            if (TrailerCheats.ForceTrial)
                EditorGUILayout.HelpBox($"La prueba solo cae desde el nivel {GameConfig.TrialsFromLevel}. Para que se note, elige cañón Normal y sin misiles.", MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Video horizontal", EditorStyles.boldLabel);
            var captions = Captions();
            if (captions.Length == 0) EditorGUILayout.HelpBox("No hay rótulos en TrailerCaptures/rotulos: el video horizontal saldrá con fondo negro.", MessageType.Warning);
            else
            {
                var names = new string[captions.Length];
                for (int i = 0; i < captions.Length; i++) names[i] = Path.GetFileNameWithoutExtension(captions[i]);
                _caption = Mathf.Clamp(_caption, 0, captions.Length - 1);
                _caption = EditorGUILayout.Popup("Rótulo", _caption, names);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!playing))
            {
                if (GUILayout.Button("Jugar nivel", GUILayout.Height(32))) { PlayLevel(ships[_ship]); FocusGame(); }
                if (GUILayout.Button("Volver al menú")) GameManager.Instance.ReturnToMenu();

                if (playing && Screen.width > Screen.height)
                    EditorGUILayout.HelpBox("La pestaña Game está en horizontal. Ponla en 1080x1920 antes de grabar.", MessageType.Warning);

                var rec = TrailerRecorder.Instance;
                bool recording = rec != null && rec.Recording;
                GUI.backgroundColor = recording ? new Color(1f, 0.45f, 0.45f) : new Color(0.6f, 1f, 0.6f);
                if (GUILayout.Button(recording ? "Detener grabación" : "Grabar", GUILayout.Height(40)))
                {
                    if (recording) rec.StopRecording();
                    else { StartRecording(captions.Length > 0 ? captions[_caption] : null); FocusGame(); }
                }
                GUI.backgroundColor = Color.white;
                if (recording) EditorGUILayout.LabelField("Grabando", $"{rec.Frames / (float)TrailerRecorder.Fps:0.0} s  ({rec.Size.x}x{rec.Size.y})");
            }
            if (!playing) EditorGUILayout.HelpBox("Pulsa Play para activar los botones.", MessageType.Warning);

            EditorGUILayout.Space();
            if (GUILayout.Button("Abrir carpeta de grabaciones"))
            {
                Directory.CreateDirectory(Folder);
                EditorUtility.RevealInFinder(Path.GetFullPath(Folder) + "/");
            }
            EditorGUILayout.EndScrollView();
        }

        void PlayLevel(ShipDefinition ship)
        {
            TrailerCheats.Active = true;
            var gm = GameManager.Instance;
            var baseState = gm.Data.GetShip(ship.id);
            // Copia temporal: no se toca el progreso guardado
            var state = new ShipState { shipId = ship.id, paintId = baseState != null ? baseState.paintId : "default" };
            if (baseState != null)
            {
                state.armorLevel = baseState.armorLevel; state.engineLevel = baseState.engineLevel;
                state.magazineLevel = baseState.magazineLevel; state.chargerLevel = baseState.chargerLevel;
            }
            state.equipped = new List<string>();
            if (CannonIds[_cannon] != null) state.equipped.Add(CannonIds[_cannon]);
            if (_missiles) state.equipped.Add("eq_missiles");
            if (_shield) state.equipped.Add("eq_shield");

            AdsManager.Instance.HideBanner();
            UIManager.Instance.ShowScreen<HUDScreen>();
            gm.Gameplay.Begin(_level, ShipStats.Compute(state), ship.id);
        }

        static void StartRecording(string captionPng)
        {
            TrailerCheats.Active = true;
            Directory.CreateDirectory(Folder);
            string basePath = Path.GetFullPath($"{Folder}/clip_{DateTime.Now:HHmmss}");
            TrailerRecorder.Ensure().StartRecording(basePath, captionPng);
        }

        /// <summary>
        /// Prueba sin abrir el juego (-executeMethod): codifica 2 s de imagen sintética en vertical y horizontal
        /// y comprueba que la composición horizontal no sale invertida.
        /// </summary>
        public static void SelfTestBatch()
        {
            int exit = 1;
            try
            {
                string dir = Folder + "/prueba";
                Directory.CreateDirectory(dir);
                const int W = 1080, H = 1920;
                var shot = new Texture2D(W, H, TextureFormat.RGBA32, false);
                var px = new Color32[W * H];
                var captions = Captions();
                using (var comp = new TrailerCompositor(W, H, captions.Length > 0 ? captions[0] : null))
                {
                    var v = TrailerRecorder.CreateEncoder(dir + "/prueba_vertical.mp4", W, H);
                    var h = TrailerRecorder.CreateEncoder(dir + "/prueba_horizontal.mp4", TrailerCompositor.WideW, TrailerCompositor.WideH);
                    for (int f = 0; f < 60; f++)
                    {
                        for (int y = 0; y < H; y++)
                            for (int x = 0; x < W; x++)
                            {
                                // Arriba rojo, abajo azul y un cuadro blanco que se mueve
                                Color32 c = y >= H - 300 ? new Color32(230, 30, 30, 255)
                                          : y < 300 ? new Color32(30, 60, 230, 255)
                                          : new Color32((byte)(x * 255 / W), 40, (byte)(y * 255 / H), 255);
                                int sx = 100 + f * 12;
                                if (x >= sx && x < sx + 160 && y >= 900 && y < 1060) c = new Color32(255, 255, 255, 255);
                                px[y * W + x] = c;
                            }
                        shot.SetPixels32(px);
                        shot.Apply(false);
                        v.AddFrame(shot);
                        h.AddFrame(comp.Compose(shot));
                    }
                    v.Dispose();
                    h.Dispose();
                    File.WriteAllBytes(dir + "/prueba_horizontal.png", comp.Frame.EncodeToPNG());
                    Color top = comp.Frame.GetPixel(960, TrailerCompositor.WideH - 20), bottom = comp.Frame.GetPixel(960, 20);
                    bool upright = top.r > 0.7f && top.b < 0.3f && bottom.b > 0.7f && bottom.r < 0.3f;
                    long sv = new FileInfo(dir + "/prueba_vertical.mp4").Length, sh = new FileInfo(dir + "/prueba_horizontal.mp4").Length;
                    Debug.Log($"[Tráiler] Prueba: arriba={top} abajo={bottom} derecho={upright} vertical={sv / 1024} KB horizontal={sh / 1024} KB");
                    if (upright && sv > 10000 && sh > 10000) exit = 0;
                }
            }
            catch (Exception e) { Debug.LogError("[Tráiler] Prueba fallida: " + e); }
            EditorApplication.Exit(exit);
        }
    }
}
