using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>Tipos de ataque de los jefes. Cada jefe combina unos cuantos en cada fase.</summary>
    public enum BossPattern
    {
        Aimed,    // ráfaga de 3 disparos dirigidos a la nave
        Fan,      // abanico de disparos hacia abajo
        Spiral,   // barrido: una serie de disparos que oscila de lado a lado
        Rain,     // lluvia de disparos verticales desde arriba
        Ring,     // anillo de disparos que sale del jefe
        GapWall   // fila de disparos con un hueco por donde hay que pasar
    }

    public enum SummonKind { None, Drones, Mines, Mixed }

    /// <summary>Una fase del jefe (cada una dura un tercio de su vida): qué ataca, con qué ritmo y qué invoca.</summary>
    public class BossPhase
    {
        public BossPattern[] patterns;
        public float interval;       // segundos entre ataques
        public float shotSpeed;      // velocidad de los disparos
        public int summonCount;      // subordinados por invocación (0 = no invoca)
        public SummonKind summon;
        public float summonEvery;    // segundos entre invocaciones
        public float moveMul = 1f;   // velocidad de movimiento del jefe
    }

    /// <summary>Una pieza dibujada del jefe: un polígono (lado derecho, x >= 0) con su brillo. Se refleja sola hacia la izquierda.</summary>
    public class BossPart
    {
        public Vector2[] pts;
        public float shade = 1f;
        public float radius;   // solo en las luces redondas: para pintarles el centro blanco
    }

    public class BossDefinition
    {
        public string id, nameKey;
        public int level;                    // nivel en el que aparece (10, 20 ... 70)
        public float width;                  // ancho del sprite en unidades de mundo
        public Vector2 hit;                  // mitad del ancho y del alto de la zona que recibe disparos
        public float fightSeconds;           // duración objetivo de la pelea (ver GameplayController.Boss)
        public float moveSpeed, moveRange;   // vaivén horizontal
        public Color hull, accent;           // tono del casco y color de las luces
        public float coreY;                  // altura del "ojo" brillante (-1..1 sobre el sprite)
        public BossPart[] parts;              // piezas del casco (de atrás hacia delante); solo el lado derecho, la izquierda es el espejo
        public BossPart[] lightParts;         // luces que brillan con el color de acento (ojos, cañones, motores)
        public BossPhase[] phases;

        public string Name => L.T(nameKey);
    }

    /// <summary>
    /// Qué jefes ha vencido cada jugador. Los jugadores que ya habían pasado un nivel de jefe antes de que existieran
    /// (v1.1.0) lo tienen "pendiente": pueden retarlo desde la Ruta Galáctica sin perder ni repetir nada de su progreso.
    /// </summary>
    public static class BossProgress
    {
        static int Bit(int bossLevel) => 1 << (bossLevel / 10 - 1);

        public static bool IsBeaten(PlayerData d, int bossLevel) => BossCatalog.IsBossLevel(bossLevel) && (d.bossesBeaten & Bit(bossLevel)) != 0;
        public static void MarkBeaten(PlayerData d, int bossLevel) { if (BossCatalog.IsBossLevel(bossLevel)) d.bossesBeaten |= Bit(bossLevel); }

        /// <summary>Niveles superados de la campaña.</summary>
        public static int Cleared(PlayerData d) => d.campaignCompleted ? GameConfig.TotalLevels : Mathf.Clamp(d.highestUnlocked - 1, 0, GameConfig.TotalLevels);

        /// <summary>El jugador ya pasó ese nivel sin haber peleado contra su jefe: puede retarlo.</summary>
        public static bool IsPending(PlayerData d, int bossLevel) => BossCatalog.IsBossLevel(bossLevel) && bossLevel <= Cleared(d) && !IsBeaten(d, bossLevel);
    }

    /// <summary>
    /// Siete jefes, uno al final de cada diez niveles (10, 20 ... 70: los antiguos niveles "Tormenta").
    /// Todos usan el mismo motor (GameplayController.Boss.cs): aquí solo hay datos.
    /// </summary>
    public static class BossCatalog
    {
        public const int Count = 7;
        public static bool IsBossLevel(int level) => level >= 10 && level <= Count * 10 && level % 10 == 0;
        public static BossDefinition ForLevel(int level) => IsBossLevel(level) ? All[level / 10 - 1] : null;

        static Vector2 P(float x, float y) => new Vector2(x, y);

        /// <summary>Pieza del casco: pares x, y del lado derecho (si toca el eje central, empieza y acaba en x = 0).</summary>
        static BossPart H(float shade, params float[] xy)
        {
            var pts = new Vector2[xy.Length / 2];
            for (int i = 0; i < pts.Length; i++) pts[i] = new Vector2(xy[i * 2], xy[i * 2 + 1]);
            return new BossPart { pts = pts, shade = shade };
        }

        /// <summary>Luz redonda (si está a la derecha del centro, aparece también a la izquierda).</summary>
        static BossPart Dot(float cx, float cy, float r)
        {
            const int n = 12;
            var pts = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            return new BossPart { pts = pts, radius = r };
        }

        static BossPhase Ph(BossPattern[] patterns, float interval, float speed, int summonCount = 0, SummonKind kind = SummonKind.None, float summonEvery = 9f, float moveMul = 1f) =>
            new BossPhase { patterns = patterns, interval = interval, shotSpeed = speed, summonCount = summonCount, summon = kind, summonEvery = summonEvery, moveMul = moveMul };

        const BossPattern A = BossPattern.Aimed, F = BossPattern.Fan, S = BossPattern.Spiral, R = BossPattern.Rain, O = BossPattern.Ring, G = BossPattern.GapWall;

        public static readonly BossDefinition[] All =
        {
            new BossDefinition
            {
                id = "destroyer", nameKey = "boss.destroyer", level = 10, width = 4.4f, hit = P(1.9f, 0.95f), fightSeconds = 50f, moveSpeed = 1.5f, moveRange = 2.1f,
                hull = new Color(0.78f, 0.8f, 0.86f), accent = new Color(1f, 0.45f, 0.2f), coreY = 0.1f,
                parts = new[]
                {
                    H(0.85f, 0, 0.8f, 0.36f, 0.8f, 0.5f, 0.55f, 0.5f, -0.3f, 0.3f, -0.55f, 0, -0.6f),                                   // cuerpo
                    H(0.65f, 0.55f, 0.75f, 0.82f, 0.75f, 0.95f, 0.5f, 0.95f, -0.45f, 0.82f, -0.62f, 0.62f, -0.62f, 0.55f, -0.4f),     // brazos
                    H(0.5f, 0.66f, -0.6f, 0.76f, -0.6f, 0.76f, -0.98f, 0.66f, -0.98f),                                                  // cañones laterales
                    H(0.5f, 0, -0.55f, 0.12f, -0.55f, 0.12f, -0.92f, 0, -0.92f),                                                        // cañón central
                    H(1f, 0, 0.98f, 0.22f, 0.98f, 0.3f, 0.78f, 0, 0.78f),                                                               // cabeza
                },
                lightParts = new[] { Dot(0, 0.1f, 0.15f), Dot(0.75f, 0.1f, 0.08f), Dot(0.75f, -0.3f, 0.06f), Dot(0.71f, -0.96f, 0.06f), Dot(0, -0.9f, 0.07f), Dot(0.12f, 0.9f, 0.05f) },
                phases = new[]
                {
                    Ph(new[] { A, F }, 2.7f, 4.4f),
                    Ph(new[] { F, A, R }, 2.3f, 4.8f, 2, SummonKind.Drones, 9f),
                    Ph(new[] { S, A, F }, 1.9f, 5.2f, 2, SummonKind.Drones, 8f, 1.4f),
                }
            },
            new BossDefinition
            {
                id = "spider", nameKey = "boss.spider", level = 20, width = 4.6f, hit = P(1.9f, 0.9f), fightSeconds = 55f, moveSpeed = 1.7f, moveRange = 2.0f,
                hull = new Color(0.72f, 0.66f, 0.85f), accent = new Color(0.55f, 1f, 0.35f), coreY = 0.05f,
                parts = new[]
                {
                    H(0.5f, 0.3f, 0.45f, 0.6f, 0.68f, 0.98f, 0.3f, 0.9f, 0.2f, 0.6f, 0.42f, 0.38f, 0.22f),                                // pata 1
                    H(0.5f, 0.34f, 0.15f, 0.75f, 0.15f, 1f, -0.2f, 0.92f, -0.28f, 0.7f, 0f, 0.34f, 0f),                                    // pata 2
                    H(0.5f, 0.3f, -0.15f, 0.62f, -0.25f, 0.82f, -0.72f, 0.74f, -0.78f, 0.55f, -0.4f, 0.3f, -0.3f),                         // pata 3
                    H(0.7f, 0, 0.92f, 0.36f, 0.82f, 0.5f, 0.45f, 0.46f, 0.08f, 0.2f, -0.05f, 0, -0.05f),                                    // abdomen
                    H(0.92f, 0, 0.12f, 0.3f, 0.12f, 0.4f, -0.2f, 0.24f, -0.48f, 0, -0.54f),                                                 // cabeza
                    H(0.4f, 0.1f, -0.48f, 0.2f, -0.48f, 0.15f, -0.9f),                                                                       // colmillos
                },
                lightParts = new[] { Dot(0.1f, -0.04f, 0.07f), Dot(0.24f, -0.12f, 0.06f), Dot(0, 0.55f, 0.11f), Dot(0.8f, 0.3f, 0.05f), Dot(0.88f, -0.22f, 0.05f) },
                phases = new[]
                {
                    Ph(new[] { F, R }, 2.5f, 4.8f),
                    Ph(new[] { A, O }, 2.1f, 5.0f, 1, SummonKind.Mines, 9f),
                    Ph(new[] { O, S, R }, 1.7f, 5.4f, 2, SummonKind.Mixed, 8f, 1.4f),
                }
            },
            new BossDefinition
            {
                id = "battleship", nameKey = "boss.battleship", level = 30, width = 4.8f, hit = P(2.3f, 1.0f), fightSeconds = 60f, moveSpeed = 1.1f, moveRange = 1.8f,
                hull = new Color(0.62f, 0.72f, 0.88f), accent = new Color(0.3f, 0.8f, 1f), coreY = 0f,
                parts = new[]
                {
                    H(0.6f, 0.6f, 0.5f, 0.95f, 0.3f, 1f, -0.2f, 0.8f, -0.55f, 0.62f, -0.45f),                                              // blindaje lateral
                    H(0.8f, 0, 0.95f, 0.3f, 0.9f, 0.55f, 0.6f, 0.62f, 0f, 0.55f, -0.5f, 0.3f, -0.85f, 0, -0.95f),                          // casco
                    H(0.45f, 0.05f, 0.1f, 0.14f, 0.1f, 0.14f, -0.72f, 0.05f, -0.72f),                                                        // cañones
                    H(1f, 0, 0.63f, 0.2f, 0.55f, 0.28f, 0.35f, 0.2f, 0.15f, 0, 0.07f),                                                      // torreta
                    H(0.95f, 0, -0.25f, 0.18f, -0.25f, 0.22f, -0.5f, 0, -0.6f),                                                              // puente
                },
                lightParts = new[] { Dot(0, 0.35f, 0.09f), Dot(0.1f, -0.42f, 0.05f), Dot(0.8f, 0.05f, 0.06f), Dot(0.7f, 0.35f, 0.05f), Dot(0.4f, 0.75f, 0.05f) },
                phases = new[]
                {
                    Ph(new[] { G, A }, 2.9f, 4.2f),
                    Ph(new[] { G, F }, 2.4f, 4.6f, 3, SummonKind.Drones, 9f),
                    Ph(new[] { G, S, R }, 1.9f, 5.0f, 3, SummonKind.Mixed, 8f, 1.3f),
                }
            },
            new BossDefinition
            {
                id = "ghost", nameKey = "boss.ghost", level = 40, width = 4.6f, hit = P(1.8f, 0.9f), fightSeconds = 60f, moveSpeed = 2.3f, moveRange = 2.2f,
                hull = new Color(0.82f, 0.92f, 0.98f), accent = new Color(0.55f, 1f, 0.95f), coreY = 0.2f,
                parts = new[]
                {
                    H(0.5f, 0.8f, 0.3f, 1f, 0f, 0.95f, -0.45f, 0.85f, -0.2f),                                                                // cuchillas de las alas
                    H(0.7f, 0.2f, 0.45f, 0.95f, 0.35f, 1f, 0.15f, 0.7f, -0.05f, 0.5f, -0.55f, 0.3f, -0.1f),                                  // alas
                    H(0.9f, 0, 0.95f, 0.14f, 0.5f, 0.3f, 0.1f, 0.2f, -0.4f, 0, -0.85f),                                                      // cuerpo
                },
                lightParts = new[] { Dot(0, 0.25f, 0.1f), Dot(0.18f, -0.05f, 0.05f), Dot(0.9f, 0.1f, 0.05f), Dot(0.6f, 0.2f, 0.04f), Dot(0, -0.55f, 0.05f) },
                phases = new[]
                {
                    Ph(new[] { A, O }, 2.2f, 5.2f),
                    Ph(new[] { O, F, R }, 1.9f, 5.5f, 1, SummonKind.Mines, 8f, 1.2f),
                    Ph(new[] { S, O, A }, 1.5f, 6.0f, 2, SummonKind.Mines, 7f, 1.5f),
                }
            },
            new BossDefinition
            {
                id = "queen", nameKey = "boss.queen", level = 50, width = 4.8f, hit = P(1.9f, 1.0f), fightSeconds = 65f, moveSpeed = 1.5f, moveRange = 2.0f,
                hull = new Color(0.98f, 0.86f, 0.55f), accent = new Color(1f, 0.82f, 0.15f), coreY = 0.0f,
                parts = new[]
                {
                    H(0.55f, 0.55f, 0.5f, 1f, 0.75f, 0.95f, 0.2f, 0.62f, 0.1f),                                                              // alas
                    H(0.75f, 0, 0.9f, 0.4f, 0.8f, 0.62f, 0.4f, 0.55f, -0.1f, 0.25f, -0.3f, 0, -0.3f),                                       // abdomen
                    H(0.6f, 0, 0.98f, 0.1f, 1f, 0.2f, 0.9f, 0.32f, 0.98f, 0.4f, 0.85f, 0.1f, 0.82f, 0, 0.82f),                               // corona
                    H(0.95f, 0, -0.1f, 0.28f, -0.1f, 0.38f, -0.45f, 0.2f, -0.7f, 0, -0.75f),                                                 // cabeza
                    H(0.4f, 0.2f, -0.6f, 0.4f, -0.65f, 0.3f, -0.98f, 0.24f, -0.8f),                                                          // mandíbulas
                },
                lightParts = new[] { Dot(0.14f, -0.35f, 0.08f), Dot(0, 0.45f, 0.16f), Dot(0.8f, 0.45f, 0.06f), Dot(0.45f, 0.2f, 0.05f), Dot(0.25f, 0.9f, 0.04f) },
                phases = new[]
                {
                    Ph(new[] { F, R }, 2.3f, 5.0f, 2, SummonKind.Drones, 7f),
                    Ph(new[] { S, O }, 1.9f, 5.4f, 3, SummonKind.Drones, 6.5f),
                    Ph(new[] { R, S, F }, 1.5f, 5.8f, 4, SummonKind.Mixed, 6f, 1.4f),
                }
            },
            new BossDefinition
            {
                id = "ai", nameKey = "boss.ai", level = 60, width = 4.6f, hit = P(1.7f, 1.0f), fightSeconds = 70f, moveSpeed = 1.9f, moveRange = 2.1f,
                hull = new Color(0.9f, 0.82f, 0.95f), accent = new Color(1f, 0.3f, 0.85f), coreY = 0.05f,
                parts = new[]
                {
                    H(0.65f, 0.5f, 0.7f, 0.95f, 0.2f, 0.95f, -0.2f, 0.5f, -0.7f, 0.52f, -0.1f, 0.52f, 0.1f),                                // paneles
                    H(0.9f, 0, 0.8f, 0.45f, 0.3f, 0.45f, -0.3f, 0, -0.8f),                                                                    // núcleo
                    H(0.5f, 0, -0.8f, 0.1f, -0.8f, 0.14f, -1f, 0, -0.95f),                                                                    // punta
                },
                lightParts = new[] { Dot(0, 0, 0.22f), Dot(0, 0.5f, 0.06f), Dot(0, -0.5f, 0.06f), Dot(0.72f, 0.3f, 0.05f), Dot(0.72f, -0.3f, 0.05f), Dot(0.88f, 0, 0.04f) },
                phases = new[]
                {
                    Ph(new[] { G, A, O }, 2.1f, 5.4f),
                    Ph(new[] { S, G, R }, 1.7f, 5.8f, 2, SummonKind.Mixed, 8f, 1.2f),
                    Ph(new[] { S, G, O, A, R }, 1.3f, 6.2f, 3, SummonKind.Mixed, 7f, 1.5f),
                }
            },
            new BossDefinition
            {
                id = "emperor", nameKey = "boss.emperor", level = 70, width = 5.0f, hit = P(2.2f, 1.05f), fightSeconds = 80f, moveSpeed = 1.8f, moveRange = 2.0f,
                hull = new Color(1f, 0.82f, 0.5f), accent = new Color(1f, 0.25f, 0.3f), coreY = 0.05f,
                parts = new[]
                {
                    H(0.5f, 0.5f, -0.15f, 0.9f, -0.2f, 0.95f, -0.7f, 0.6f, -0.45f),                                                          // alas inferiores
                    H(0.6f, 0.4f, 0.7f, 1f, 0.88f, 0.95f, 0.3f, 0.7f, -0.1f, 0.45f, -0.05f),                                                 // alas superiores
                    H(0.85f, 0, 0.85f, 0.3f, 0.75f, 0.45f, 0.3f, 0.4f, -0.4f, 0.2f, -0.8f, 0, -0.9f),                                        // cuerpo
                    H(1f, 0, 0.98f, 0.08f, 1f, 0.16f, 0.9f, 0.26f, 1f, 0.32f, 0.85f, 0.3f, 0.78f, 0, 0.78f),                                  // corona
                },
                lightParts = new[] { Dot(0, 0.1f, 0.16f), Dot(0.12f, -0.4f, 0.05f), Dot(0.8f, 0.55f, 0.07f), Dot(0.75f, -0.4f, 0.05f), Dot(0.16f, 0.9f, 0.04f) },
                phases = new[]
                {
                    Ph(new[] { F, G, A }, 1.9f, 5.6f),
                    Ph(new[] { O, S, R, G }, 1.6f, 6.0f, 3, SummonKind.Mixed, 7f, 1.2f),
                    Ph(new[] { S, G, O, A, R, F }, 1.2f, 6.5f, 4, SummonKind.Mixed, 6f, 1.5f),
                }
            },
        };
    }
}
