using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Música de fondo de los niveles, sintetizada por código (sin archivos de audio).
    /// 10 temas: uno por mundo (cada 7 niveles, ver WorldCatalog). El último (64-70) es de suspenso.
    /// El volumen es bajo a propósito para que se oigan los efectos de sonido.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        /// <summary>Volumen de la música durante el juego (0..1). Los efectos suenan a 1.</summary>
        const float GameVolume = 0.2f;
        const float PausedVolume = 0.07f;
        const int Rate = 22050;
        const int Bars = 16;

        AudioSource _src;
        int _loadedTrack = -1;
        int _generatingTrack = -1;
        Task<float[]> _task;
        float _volume;

        public const int SampleRate = Rate;
        public static int TrackCount => Tracks.Length;
        /// <summary>Genera las muestras de un tema (para exportarlo o revisarlo desde el Editor).</summary>
        public static float[] RenderTrack(int index) => Render(Tracks[index]);

        public static int TrackForLevel(int level) =>
            Mathf.Clamp(WorldCatalog.WorldForLevel(level), 0, Tracks.Length - 1);

        void Awake()
        {
            Instance = this;
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.loop = true;
            _src.volume = 0f;
            _src.priority = 0;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Gameplay == null || gm.Data == null) return;
            var state = gm.Gameplay.State;
            bool inLevel = state == GameplayController.RunState.Running || state == GameplayController.RunState.Paused;

            // Tema que corresponde al nivel actual: se genera en segundo plano la primera vez
            if (inLevel && gm.Data.musicOn)
            {
                int wanted = TrackForLevel(gm.Gameplay.Level);
                if (wanted != _loadedTrack && _generatingTrack != wanted) StartGenerating(wanted);
            }

            if (_task != null && _task.IsCompleted)
            {
                if (_task.Status == TaskStatus.RanToCompletion) ApplyClip(_generatingTrack, _task.Result);
                _task = null;
                _generatingTrack = -1;
            }

            float target = 0f;
            if (gm.Data.musicOn && _src.clip != null && inLevel && _loadedTrack == TrackForLevel(gm.Gameplay.Level))
                target = state == GameplayController.RunState.Running ? GameVolume : PausedVolume;

            if (target > 0f && !_src.isPlaying) { _src.time = 0f; _src.Play(); }
            _volume = Mathf.MoveTowards(_volume, target, Time.unscaledDeltaTime * 0.35f);
            _src.volume = _volume;
            if (_volume <= 0.0001f && _src.isPlaying) _src.Stop();
        }

        void StartGenerating(int track)
        {
            _generatingTrack = track;
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL no tiene hilos: se genera en el momento (pequeña pausa solo la primera vez).
            _task = Task.FromResult(Render(Tracks[track]));
#else
            var def = Tracks[track];
            _task = Task.Run(() => Render(def));
#endif
        }

        void ApplyClip(int track, float[] samples)
        {
            if (track < 0 || samples == null) return;
            if (_src.isPlaying) _src.Stop();
            var old = _src.clip;
            var clip = AudioClip.Create("music_" + track, samples.Length, 1, Rate, false);
            clip.SetData(samples, 0);
            _src.clip = clip;
            _loadedTrack = track;
            _volume = 0f;
            if (old != null) Destroy(old); // solo se guarda un tema en memoria
        }

        // =====================================================================
        //  DEFINICIÓN DE LOS 10 TEMAS (uno por mundo, en el mismo orden que WorldCatalog)
        // =====================================================================
        class Track
        {
            public int bpm;
            public int root;            // nota MIDI de la tónica
            public int[] scale;         // 7 intervalos en semitonos
            public int[] chords;        // grado de la escala por compás (8 compases, se repite con variación)
            public int bass;            // 0 negras, 1 corcheas con octava, 2 semicorcheas, 3 latido
            public int arpRate;         // 0 sin arpegio, 2 corcheas, 1 semicorcheas
            public float pad, lead, arp;// niveles de mezcla
            public int drums;           // 0 suave, 1 normal, 2 intenso, 3 tic-tac de tensión
            public int leadWave;        // 0 seno, 1 triángulo, 2 cuadrada suave, 3 sierra
            public bool suspense;       // alarma, semitonos y subidas de ruido
            public int seed;
        }

        static readonly int[] Major = { 0, 2, 4, 5, 7, 9, 11 };
        static readonly int[] Minor = { 0, 2, 3, 5, 7, 8, 10 };
        static readonly int[] Dorian = { 0, 2, 3, 5, 7, 9, 10 };
        static readonly int[] Phrygian = { 0, 1, 3, 5, 7, 8, 10 };
        static readonly int[] Locrian = { 0, 1, 3, 5, 6, 8, 10 };
        static readonly int[] Lydian = { 0, 2, 4, 6, 7, 9, 11 };
        static readonly int[] Mixolydian = { 0, 2, 4, 5, 7, 9, 10 };
        static readonly int[] HarmonicMinor = { 0, 2, 3, 5, 7, 8, 11 };

        static readonly Track[] Tracks =
        {
            // Mundo 1 (1-7) "Cinturón Exterior": despegue alegre y ligero
            new Track { bpm = 108, root = 60, scale = Major, chords = new[] { 0, 4, 5, 3, 0, 4, 3, 4 }, bass = 0, arpRate = 2, pad = 0.9f, lead = 1f, arp = 0.8f, drums = 0, leadWave = 1, seed = 11 },
            // Mundo 2 (8-14) "Anillos de Hielo": cristalino y luminoso (nuevo)
            new Track { bpm = 112, root = 59, scale = Lydian, chords = new[] { 0, 4, 1, 4, 0, 5, 1, 4 }, bass = 0, arpRate = 1, pad = 1f, lead = 0.8f, arp = 1f, drums = 0, leadWave = 0, seed = 29 },
            // Mundo 3 (15-21) "Nebulosa Carmesí": aventura
            new Track { bpm = 116, root = 57, scale = Minor, chords = new[] { 0, 5, 2, 6, 0, 5, 3, 4 }, bass = 1, arpRate = 2, pad = 0.9f, lead = 1f, arp = 0.9f, drums = 1, leadWave = 1, seed = 23 },
            // Mundo 4 (22-28) "Campo Minado": misterioso con ritmo
            new Track { bpm = 122, root = 62, scale = Dorian, chords = new[] { 0, 3, 0, 6, 0, 3, 5, 6 }, bass = 1, arpRate = 1, pad = 1f, lead = 0.9f, arp = 0.8f, drums = 1, leadWave = 2, seed = 37 },
            // Mundo 5 (29-35) "Llamarada Solar": enérgico y brillante (nuevo)
            new Track { bpm = 126, root = 58, scale = Mixolydian, chords = new[] { 0, 6, 3, 0, 0, 6, 4, 4 }, bass = 1, arpRate = 2, pad = 0.8f, lead = 1f, arp = 0.9f, drums = 2, leadWave = 3, seed = 47 },
            // Mundo 6 (36-42) "Estación Abandonada": más tenso
            new Track { bpm = 128, root = 64, scale = Minor, chords = new[] { 0, 3, 5, 4, 0, 6, 5, 4 }, bass = 2, arpRate = 2, pad = 0.8f, lead = 1f, arp = 0.9f, drums = 2, leadWave = 2, seed = 41 },
            // Mundo 7 (43-49) "Corredor de Láseres": rápido y electrónico
            new Track { bpm = 134, root = 54, scale = Minor, chords = new[] { 0, 0, 5, 6, 0, 0, 3, 4 }, bass = 2, arpRate = 1, pad = 0.7f, lead = 1f, arp = 1f, drums = 2, leadWave = 3, seed = 53 },
            // Mundo 8 (50-56) "Mar de Plasma": exótico e intenso (nuevo)
            new Track { bpm = 137, root = 56, scale = HarmonicMinor, chords = new[] { 0, 5, 3, 4, 0, 5, 1, 4 }, bass = 2, arpRate = 1, pad = 0.9f, lead = 1f, arp = 0.9f, drums = 2, leadWave = 2, seed = 61 },
            // Mundo 9 (57-63) "Frontera Oscura": oscuro y pesado
            new Track { bpm = 140, root = 60, scale = Phrygian, chords = new[] { 0, 1, 0, 6, 0, 1, 5, 1 }, bass = 2, arpRate = 1, pad = 0.9f, lead = 0.9f, arp = 0.9f, drums = 2, leadWave = 3, seed = 67 },
            // Mundo 10 (64-70) "Núcleo Galáctico": SUSPENSO. Latido, tic-tac, alarma y disonancia sin resolver.
            new Track { bpm = 152, root = 49, scale = Locrian, chords = new[] { 0, 0, 1, 0, 0, 0, 4, 1 }, bass = 3, arpRate = 1, pad = 0.9f, lead = 1f, arp = 1f, drums = 3, leadWave = 2, suspense = true, seed = 79 },
        };

        // =====================================================================
        //  SINTETIZADOR (solo matemáticas: puede ejecutarse en otro hilo)
        // =====================================================================
        static double Hz(int midi) => 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

        static int Note(Track t, int degree, int octaveShift = 0)
        {
            int oct = (int)Math.Floor(degree / 7.0);
            int idx = ((degree % 7) + 7) % 7;
            return t.root + t.scale[idx] + 12 * (oct + octaveShift);
        }

        static double Wave(int kind, double ph)
        {
            switch (kind)
            {
                case 1: return Math.Asin(Math.Sin(ph)) * (2.0 / Math.PI);                                  // triángulo
                case 2: return (Math.Sin(ph) + Math.Sin(3 * ph) / 3 + Math.Sin(5 * ph) / 5) * 0.8;       // cuadrada suave
                case 3: return (Math.Sin(ph) - Math.Sin(2 * ph) / 2 + Math.Sin(3 * ph) / 3 - Math.Sin(4 * ph) / 4) * 0.7; // sierra
                default: return Math.Sin(ph);
            }
        }

        /// <summary>Añade una nota. Las colas que pasan del final vuelven al principio (bucle sin cortes).</summary>
        static void Tone(float[] b, int start, int len, double freq, double amp, int wave,
            double attack, double decay, double release, double vibHz = 0, double vibDepth = 0)
        {
            int n = b.Length;
            int rel = Math.Max(1, (int)(release * Rate));
            int att = Math.Max(1, (int)(attack * Rate));
            double ph = 0, env = 1, decayMul = Math.Exp(-decay / Rate), step = 2 * Math.PI * freq / Rate;
            for (int i = 0; i < len + rel; i++)
            {
                double a = env * amp;
                if (i < att) a *= i / (double)att;
                if (i >= len) a *= 1.0 - (i - len) / (double)rel;
                double s = Wave(wave, ph) * a;
                b[((start + i) % n + n) % n] += (float)s;
                ph += vibDepth > 0 ? step * (1 + vibDepth * Math.Sin(2 * Math.PI * vibHz * i / Rate)) : step;
                env *= decayMul;
            }
        }

        static void Kick(float[] b, int start, double amp)
        {
            int n = b.Length, len = (int)(0.16 * Rate);
            double ph = 0;
            for (int i = 0; i < len; i++)
            {
                double t = i / (double)Rate;
                double f = 45 + 95 * Math.Exp(-t * 28);
                ph += 2 * Math.PI * f / Rate;
                b[(start + i) % n] += (float)(Math.Sin(ph) * Math.Exp(-t * 16) * amp);
            }
        }

        static void Noise(float[] b, int start, double seconds, double decay, double amp, bool bright, ref uint rng, bool rising = false)
        {
            int n = b.Length, len = (int)(seconds * Rate);
            double prev = 0;
            for (int i = 0; i < len; i++)
            {
                rng = rng * 1664525u + 1013904223u;
                double w = (rng >> 8) / 8388608.0 - 1.0;
                double s = bright ? (w - prev) * 0.7 : (w + prev) * 0.5; // agudo (platillo) o grave
                prev = w;
                double t = i / (double)Rate;
                double env = rising ? Math.Pow(i / (double)len, 2.2) : Math.Exp(-t * decay);
                b[(start + i) % n] += (float)(s * env * amp);
            }
        }

        static float[] Render(Track t)
        {
            double stepSec = 60.0 / t.bpm / 4.0;           // una semicorchea
            int stepLen = (int)Math.Round(stepSec * Rate);
            int total = stepLen * 16 * Bars;
            var b = new float[total];
            uint rng = (uint)(t.seed * 7919);

            int prevDegree = 4;
            for (int bar = 0; bar < Bars; bar++)
            {
                int chord = t.chords[bar % t.chords.Length];
                int barStart = bar * 16 * stepLen;
                bool second = bar >= Bars / 2;   // segunda mitad: variación

                // ---- Colchón de acordes ----
                if (t.pad > 0)
                {
                    int[] tones = t.suspense ? new[] { chord, chord + 1, chord + 4 } : new[] { chord, chord + 2, chord + 4 };
                    foreach (int tone in tones)
                    {
                        double f = Hz(Note(t, tone, -1));
                        Tone(b, barStart, 16 * stepLen, f, 0.055 * t.pad, 0, 0.12, 0.25, 0.25);
                        Tone(b, barStart, 16 * stepLen, f * 1.006, 0.045 * t.pad, 1, 0.12, 0.25, 0.25);
                    }
                }

                // ---- Bajo ----
                double bassHz = Hz(Note(t, chord, -2));
                for (int s = 0; s < 16; s++)
                {
                    int at = barStart + s * stepLen;
                    switch (t.bass)
                    {
                        case 0: if (s % 4 == 0) Tone(b, at, stepLen * 3, bassHz, 0.3, 1, 0.005, 3.5, 0.05); break;
                        case 1: if (s % 2 == 0) Tone(b, at, stepLen * 2 - 200, s % 4 == 2 ? bassHz * 2 : bassHz, 0.27, 1, 0.004, 5, 0.03); break;
                        case 2: Tone(b, at, stepLen - 150, (s % 8 == 6) ? bassHz * 2 : bassHz, s % 4 == 0 ? 0.26 : 0.19, 3, 0.003, 9, 0.02); break;
                        default: // latido: "tum-tum ... tum-tum"
                            if (s % 8 == 0 || s % 8 == 2) Tone(b, at, stepLen * 2, bassHz, s % 8 == 0 ? 0.36 : 0.26, 0, 0.004, 7, 0.05);
                            break;
                    }
                }

                // ---- Arpegio / ostinato ----
                if (t.arpRate > 0)
                {
                    int[] pattern = t.suspense ? new[] { 0, 1, 0, 1, 0, 1, 0, 4 } : new[] { 0, 2, 4, 7, 4, 2, 4, 2 };
                    int every = t.arpRate == 1 ? 1 : 2;
                    int lift = t.suspense ? (bar % 8) / 2 : 0;  // el ostinato sube: aumenta la tensión
                    for (int s = 0; s < 16; s += every)
                    {
                        int deg = chord + pattern[(s / every) % pattern.Length];
                        int at = barStart + s * stepLen;
                        Tone(b, at, stepLen * every - 120, Hz(Note(t, deg) + lift), 0.075 * t.arp, 2, 0.003, 14, 0.03);
                    }
                }

                // ---- Melodía ----
                if (t.lead > 0)
                {
                    if (t.suspense)
                    {
                        // Alarma: nota aguda con trémolo que alterna con su tritono, sin resolver nunca
                        int alarm = Note(t, 0, 1) + ((bar / 2) % 2 == 0 ? 0 : 6);
                        Tone(b, barStart, 16 * stepLen, Hz(alarm), 0.07 * t.lead, 0, 0.3, 0.1, 0.3, 7.5, 0.012);
                        if (bar % 2 == 1) // golpes disonantes al final de cada dos compases
                            for (int k = 0; k < 3; k++)
                                Tone(b, barStart + (10 + k * 2) * stepLen, stepLen, Hz(Note(t, 1, 1) + k), 0.1 * t.lead, 2, 0.002, 12, 0.03);
                    }
                    else
                    {
                        var r = second ? new System.Random(t.seed * 31 + bar) : new System.Random(t.seed + bar % 4);
                        int s = 0;
                        while (s < 16)
                        {
                            int len = new[] { 2, 2, 4, 4, 6 }[r.Next(5)];
                            if (s + len > 16) len = 16 - s;
                            bool rest = r.NextDouble() < (second ? 0.12 : 0.22);
                            if (!rest)
                            {
                                // En tiempos fuertes se prefieren notas del acorde; el resto se mueve por pasos
                                int deg = s % 8 == 0
                                    ? chord + new[] { 0, 2, 4, 7 }[r.Next(4)]
                                    : prevDegree + r.Next(-2, 3);
                                deg = Math.Max(chord - 1, Math.Min(chord + 9, deg));
                                prevDegree = deg;
                                Tone(b, barStart + s * stepLen, len * stepLen - 250, Hz(Note(t, deg, 1)),
                                    0.11 * t.lead, t.leadWave, 0.008, 2.2, 0.06, 5.5, 0.004);
                            }
                            s += len;
                        }
                    }
                }

                // ---- Batería ----
                for (int s = 0; s < 16; s++)
                {
                    int at = barStart + s * stepLen;
                    switch (t.drums)
                    {
                        case 0:
                            if (s == 0 || s == 8) Kick(b, at, 0.42);
                            if (s % 4 == 2) Noise(b, at, 0.03, 90, 0.05, true, ref rng);
                            break;
                        case 1:
                            if (s == 0 || s == 8 || s == 10) Kick(b, at, 0.5);
                            if (s == 4 || s == 12) Noise(b, at, 0.12, 28, 0.2, false, ref rng);
                            if (s % 2 == 0) Noise(b, at, 0.03, 100, 0.06, true, ref rng);
                            break;
                        case 2:
                            if (s % 4 == 0 || s == 14) Kick(b, at, 0.55);
                            if (s == 4 || s == 12) Noise(b, at, 0.13, 26, 0.24, false, ref rng);
                            Noise(b, at, 0.025, 110, s % 2 == 0 ? 0.07 : 0.04, true, ref rng);
                            break;
                        default: // tensión: bombo constante + tic-tac de reloj
                            if (s % 4 == 0) Kick(b, at, 0.5);
                            Noise(b, at, 0.012, 260, s % 4 == 0 ? 0.12 : 0.07, true, ref rng);
                            if (s == 12 && bar % 2 == 1) Noise(b, at, 0.16, 22, 0.22, false, ref rng);
                            break;
                    }
                }

                // ---- Subida de ruido antes de repetir la frase (solo suspenso) ----
                if (t.suspense && bar % 8 == 6)
                    Noise(b, barStart, 32 * stepSec, 0, 0.16, true, ref rng, rising: true);
            }

            // Mezcla final: saturación suave y normalización
            float peak = 0f;
            for (int i = 0; i < total; i++)
            {
                b[i] = (float)Math.Tanh(b[i] * 1.4);
                float a = Math.Abs(b[i]);
                if (a > peak) peak = a;
            }
            if (peak > 0.0001f)
            {
                float k = 0.85f / peak;
                for (int i = 0; i < total; i++) b[i] *= k;
            }
            return b;
        }
    }
}
