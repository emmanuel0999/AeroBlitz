using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AeroBlitz
{
    /// <summary>
    /// Controla una partida: nave, obstáculos, disparos, colisiones y fin de nivel.
    /// Todo se mueve manualmente (sin física) para que sea ligero y determinista en móvil.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        public enum RunState { Idle, Running, Paused, Dying, AwaitingRevive, Ended }

        class Obstacle
        {
            public ObstacleType type;
            public Transform tr;
            public SpriteRenderer sr;
            public Vector2 vel;
            public float radius;          // si no es caja
            public Vector2 half;          // si es caja
            public bool box;
            public int hp;
            public bool destructible;
            public bool hitPlayer;
            public float spin;
            public float phase;
            public float flash;
            public Color baseColor = Color.white;
        }

        class Projectile { public Transform tr; public Vector2 vel; public int pierce; public HashSet<Obstacle> hits; }
        class Pickup { public Transform tr; public bool coin; public PowerUpType type; public string trial; public Vector2 vel; public float phase, radius; }
        class Missile { public Transform tr; public Vector2 vel; public Obstacle target; public float life; }
        class Particle { public Transform tr; public SpriteRenderer sr; public Vector2 vel; public float life, maxLife; }

        public RunState State { get; private set; } = RunState.Idle;
        public int Level { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth => _stats.maxHealth;
        public int ShieldHits { get; private set; }
        public float Progress01 => _cfg == null ? 0 : Mathf.Clamp01(_time / _cfg.duration);
        public WeaponSystem Weapon { get; } = new WeaponSystem();
        public int Destroyed { get; private set; }
        public int KillPoints { get; private set; }

        // Monedas y mejoras temporales (desde el nivel GameConfig.PickupsFromLevel)
        public bool PickupsEnabled { get; private set; }
        /// <summary>Monedas recogidas en este nivel: solo se suman si se completa.</summary>
        public int RunCoins { get; private set; }
        public int RunCoinPickups { get; private set; } // cantidad de monedas recogidas (para las misiones)
        public bool TookDamage { get; private set; }     // perdió vida en este nivel (los escudos no cuentan)
        public float TempShieldTime { get; private set; }

        // Cañones y misiles (equipamiento de pago o prueba en partida desde el nivel 25)
        public int TrialCannonTier { get; private set; }
        public string TrialProductId { get; private set; }
        public int CannonTier => Mathf.Max(_stats.cannonTier, TrialCannonTier);
        public bool MissilesEnabled { get; private set; }
        public int MissilesInLoad { get; private set; }
        public float MissileReloadTime { get; private set; }
        int _missileReloadsLeft;
        readonly List<Missile> _missiles = new List<Missile>();
        bool _trialPending;
        float _trialAt;
        string _trialProduct;
        static readonly string[] TrialProducts = { ProductIds.EqTripleCannon, ProductIds.EqPlasmaCannon, ProductIds.EqMissiles };
        public float OverchargeTime { get; private set; }
        public float MagnetTime { get; private set; }

        Camera _cam;
        Transform _world;
        Transform _ship;
        SpriteRenderer _shipSr, _shieldSr;
        Transform _flame;
        LevelConfig _cfg;
        ShipStats _stats;
        float _time, _spawnTimer, _invuln, _dyingTimer, _shake;
        bool _revived;
        Vector2 _dragOffset;
        bool _dragging;
        float _halfW, _halfH;

        readonly List<Obstacle> _obstacles = new List<Obstacle>();
        readonly List<Projectile> _projectiles = new List<Projectile>();
        readonly List<Particle> _particles = new List<Particle>();
        readonly List<Pickup> _pickups = new List<Pickup>();
        float _coinTimer, _coinInterval, _powerUpTimer;
        int _coinValue;
        WorldBackdrop _backdrop;
        readonly Stack<Particle> _particlePool = new Stack<Particle>();

        const float ShipRadius = 0.42f;
        const float MaxPlayHalfWidth = 5f;

        void Awake()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                _cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            _cam.orthographic = true;
            _cam.orthographicSize = 8f;
            _cam.transform.position = new Vector3(0, 0, -10);
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.02f, 0.03f, 0.09f);

            _world = new GameObject("World").transform;
            _world.SetParent(transform);
            _backdrop = new GameObject("Backdrop").AddComponent<WorldBackdrop>();
            _backdrop.transform.SetParent(_world);
            _backdrop.Build(_cam);
            _backdrop.Apply(0);
        }

        void UpdateBounds()
        {
            _halfH = _cam.orthographicSize;
            _halfW = Mathf.Min(_halfH * _cam.aspect, MaxPlayHalfWidth);
        }

        // ---------------- CICLO DE VIDA DE LA PARTIDA ----------------
        public void Begin(int level, ShipStats stats, string shipId)
        {
            Stop();
            UpdateBounds();
            Level = level;
            _cfg = LevelDatabase.Get(level);
            _backdrop.ApplyForLevel(level); // cada 7 niveles cambia el mundo: fondo, nombre y musica
            _stats = stats;
            Health = stats.maxHealth;
            ShieldHits = stats.shieldHits;
            Weapon.Setup(stats);
            _time = 0; _spawnTimer = 1.2f; _invuln = 1f; _revived = false;
            Destroyed = 0; KillPoints = 0; TookDamage = false;
            SetupPickups(level);
            SetupMissiles(stats.missiles);

            _ship = new GameObject("PlayerShip").transform;
            _ship.SetParent(_world);
            _ship.position = new Vector3(0, -_halfH + 2.5f, 0);
            _shipSr = _ship.gameObject.AddComponent<SpriteRenderer>();
            _shipSr.sprite = SpriteFactory.Ship(ShipCatalog.Get(shipId));
            _shipSr.color = stats.color;
            _shipSr.sortingOrder = 10;

            var cockpit = new GameObject("Cockpit").AddComponent<SpriteRenderer>();
            cockpit.transform.SetParent(_ship, false);
            cockpit.transform.localPosition = new Vector3(0, 0.12f, 0);
            cockpit.sprite = SpriteFactory.Cockpit;
            cockpit.sortingOrder = 11;

            var flame = new GameObject("Flame").AddComponent<SpriteRenderer>();
            flame.transform.SetParent(_ship, false);
            flame.transform.localPosition = new Vector3(0, -0.65f, 0);
            flame.sprite = SpriteFactory.Flame;
            flame.sortingOrder = 9;
            _flame = flame.transform;

            _shieldSr = new GameObject("Shield").AddComponent<SpriteRenderer>();
            _shieldSr.transform.SetParent(_ship, false);
            _shieldSr.transform.localScale = Vector3.one * 1.9f;
            _shieldSr.sprite = SpriteFactory.Glow;
            _shieldSr.color = new Color(0.3f, 0.7f, 1f, 0.45f);
            _shieldSr.sortingOrder = 12;
            _shieldSr.enabled = ShieldHits > 0;

            State = RunState.Running;
        }

        public void Stop()
        {
            foreach (var o in _obstacles) Destroy(o.tr.gameObject);
            foreach (var p in _projectiles) Destroy(p.tr.gameObject);
            foreach (var p in _particles) Destroy(p.tr.gameObject);
            foreach (var k in _pickups) Destroy(k.tr.gameObject);
            foreach (var m in _missiles) Destroy(m.tr.gameObject);
            _missiles.Clear();
            _obstacles.Clear(); _projectiles.Clear(); _particles.Clear(); _pickups.Clear();
            Weapon.ChargeRateMultiplier = 1f;
            if (_ship != null) Destroy(_ship.gameObject);
            _ship = null;
            State = RunState.Idle;
            _dragging = false;
        }

        public void Pause()
        {
            if (State != RunState.Running) return;
            State = RunState.Paused;
            UIManager.Instance.ShowPause();
        }

        public void PauseIfRunning() { if (State == RunState.Running) Pause(); }

        public void Resume()
        {
            if (State == RunState.Paused) State = RunState.Running;
        }

        /// <summary>El jugador abandona: cuenta como nivel fallido.</summary>
        public void Abandon()
        {
            int lvl = Level;
            Stop();
            GameManager.Instance.OnLevelFailed(lvl);
        }

        public void Revive()
        {
            _revived = true;
            Health = _stats.maxHealth * GameConfig.ReviveHealthPercent;
            _invuln = 2.5f;
            // Limpia obstáculos cercanos para que el jugador pueda recuperarse
            for (int i = _obstacles.Count - 1; i >= 0; i--)
                if (_obstacles[i].tr.position.y < _halfH * 0.2f) RemoveObstacle(i, true);
            _ship.gameObject.SetActive(true);
            State = RunState.Running;
        }

        public void DeclineRevive()
        {
            int lvl = Level;
            Stop();
            GameManager.Instance.OnLevelFailed(lvl);
        }

        public void FirePressed()
        {
            if (State != RunState.Running) return;
            if (!Weapon.TryFire()) return;
            AudioFX.Instance.Play(AudioFX.Sfx.Shoot, 0.7f);
            Vector2 pos = _ship.position + Vector3.up * 0.7f;
            switch (CannonTier)
            {
                case 0: SpawnProjectile(pos); break;
                case 1: // Cañón Doble
                    SpawnProjectile(pos + Vector2.left * 0.3f);
                    SpawnProjectile(pos + Vector2.right * 0.3f);
                    break;
                case 2: // Cañón Triple: abanico de 3
                    foreach (float a in new[] { -16f, 0f, 16f }) SpawnProjectile(pos, Fan(a));
                    break;
                default: // Cañón de Plasma: abanico de 5 que atraviesa rompibles
                    foreach (float a in new[] { -30f, -15f, 0f, 15f, 30f }) SpawnProjectile(pos, Fan(a), GameConfig.PlasmaPierce, true);
                    break;
            }
        }

        static Vector2 Fan(float angleDeg)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * 16f;
        }

        // ---------------- UPDATE ----------------
        void Update()
        {
            float dt = Time.deltaTime;
            UpdateStars(dt, State == RunState.Running ? 1f : 0.3f);
            UpdateParticles(dt);
            UpdateShake(dt);

            if (State == RunState.Dying)
            {
                _dyingTimer -= dt;
                if (_dyingTimer <= 0)
                {
                    State = RunState.AwaitingRevive;
                    UIManager.Instance.ShowRevive(!_revived, Level);
                }
                return;
            }
            if (State != RunState.Running) return;

            _time += dt;
            HandleInput(dt);
            Weapon.Tick(dt);

            // Generación de obstáculos (se detiene poco antes del final)
            if (_time < _cfg.duration - 2.5f)
            {
                _spawnTimer -= dt;
                if (_spawnTimer <= 0)
                {
                    SpawnWave();
                    _spawnTimer = _cfg.spawnInterval * Random.Range(0.8f, 1.2f);
                }
            }

            if (PickupsEnabled) UpdatePickupSpawns(dt);

            UpdateProjectiles(dt);
            UpdateMissiles(dt);
            UpdateObstacles(dt);
            if (State == RunState.Running) { UpdatePickups(dt); UpdatePowerUpTimers(dt); }
            UpdateShipVisuals(dt);

            if (_time >= _cfg.duration)
            {
                State = RunState.Ended;
                var r = new LevelResult { level = Level, destroyed = Destroyed, killPoints = KillPoints, healthPercent = Health / _stats.maxHealth, pickupCoins = RunCoins, trialProductId = TrialProductId,
                                          coinPickups = RunCoinPickups, tookDamage = TookDamage };
                Stop();
                State = RunState.Ended;
                GameManager.Instance.OnLevelCompleted(r);
            }
        }

        void HandleInput(float dt)
        {
            Vector2 pos = _ship.position;
            Vector2 target = pos;
            bool hasTarget = false;

            // Táctil / ratón: arrastrar en cualquier parte que no sea un botón
            bool down = false, held = false;
            Vector2 screen = Vector2.zero;
            int pointerId = -1;
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                screen = t.position; pointerId = t.fingerId;
                down = t.phase == TouchPhase.Began;
                held = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
            }
            else
            {
                screen = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
            }

            if (down)
            {
                bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
                _dragging = !overUI;
                if (_dragging) _dragOffset = pos - (Vector2)_cam.ScreenToWorldPoint(screen);
            }
            if (!held) _dragging = false;
            if (_dragging)
            {
                target = (Vector2)_cam.ScreenToWorldPoint(screen) + _dragOffset;
                hasTarget = true;
            }

            // Teclado (PC / editor)
            var axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (axis.sqrMagnitude > 0.01f) { target = pos + axis.normalized * 2f; hasTarget = true; }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J)) FirePressed();
            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.K)) MissilePressed();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) { Pause(); return; }

            float maxStep = _stats.speed * 1.6f * dt;
            Vector2 newPos = hasTarget ? Vector2.MoveTowards(pos, target, maxStep) : pos;
            newPos.x = Mathf.Clamp(newPos.x, -_halfW + 0.6f, _halfW - 0.6f);
            newPos.y = Mathf.Clamp(newPos.y, -_halfH + 1.3f, _halfH * 0.35f);
            float vx = (newPos.x - pos.x) / Mathf.Max(dt, 0.0001f);
            _ship.position = newPos;
            _ship.rotation = Quaternion.Lerp(_ship.rotation, Quaternion.Euler(0, 0, -vx * 2.2f), 10f * dt);
        }

        void UpdateShipVisuals(float dt)
        {
            _invuln -= dt;
            _shipSr.enabled = _invuln <= 0 || Mathf.Repeat(Time.time * 12f, 1f) > 0.35f;
            _shieldSr.enabled = ShieldHits > 0 || TempShieldTime > 0;
            // Escudo temporal: dorado y parpadea en sus últimos 2 segundos
            _shieldSr.color = TempShieldTime > 0 ? new Color(1f, 0.85f, 0.35f, TempShieldTime < 2f && Mathf.Repeat(Time.time * 6f, 1f) < 0.4f ? 0.15f : 0.5f)
                                                 : new Color(0.3f, 0.7f, 1f, 0.45f);
            if (_shieldSr.enabled) _shieldSr.transform.localScale = Vector3.one * (1.85f + Mathf.Sin(Time.time * 6f) * 0.08f);
            _flame.localScale = new Vector3(0.9f, 0.8f + Random.value * 0.5f, 1);
        }

        void UpdateStars(float dt, float mult)
        {
            float speedBoost = _cfg != null && State == RunState.Running ? _cfg.obstacleSpeed / 3f : 1f;
            _backdrop.Tick(dt, speedBoost, mult);
        }

        // ---------------- OBSTÁCULOS ----------------
        void SpawnWave()
        {
            // Compuertas láser: ocupan toda la fila, salen solas
            if (_cfg.allowLaserGates && Random.value < 0.12f)
            {
                SpawnLaserGate();
                return;
            }
            int count = Random.Range(1, _cfg.maxPerWave + 1);
            var usedX = new List<float>();
            for (int i = 0; i < count; i++)
            {
                float x = 0;
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    x = Random.Range(-_halfW + 0.8f, _halfW - 0.8f);
                    bool ok = true;
                    foreach (var ux in usedX) if (Mathf.Abs(ux - x) < 1.8f) ok = false;
                    if (ok) break;
                }
                usedX.Add(x);
                SpawnObstacle(PickType(), x, _halfH + 1.5f + i * 0.6f);
            }
        }

        ObstacleType PickType()
        {
            if (Random.value < _cfg.destructibleRatio)
            {
                float r = Random.value * (3f + (_cfg.allowBigAsteroids ? 1.5f : 0f) + (_cfg.allowDrones ? 1.5f : 0f));
                if (r < 3f) return ObstacleType.SmallAsteroid;
                r -= 3f;
                if (_cfg.allowBigAsteroids && r < 1.5f) return ObstacleType.BigAsteroid;
                return _cfg.allowDrones ? ObstacleType.Drone : ObstacleType.SmallAsteroid;
            }
            if (_cfg.allowMines && Random.value < 0.4f) return ObstacleType.Mine;
            return ObstacleType.MetalWall;
        }

        Obstacle NewObstacle(ObstacleType type, Sprite sprite, Vector2 pos)
        {
            var go = new GameObject(type.ToString());
            go.transform.SetParent(_world);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;
            var o = new Obstacle
            {
                type = type, tr = go.transform, sr = sr,
                destructible = LevelDatabase.IsDestructible(type),
                hp = LevelDatabase.BaseHitPoints(type),
                phase = Random.value * 10f
            };
            _obstacles.Add(o);
            return o;
        }

        void SpawnObstacle(ObstacleType type, float x, float y)
        {
            float speed = _cfg.obstacleSpeed * Random.Range(0.85f, 1.15f);
            var pos = new Vector2(x, y);
            Obstacle o;
            switch (type)
            {
                case ObstacleType.SmallAsteroid:
                    o = NewObstacle(type, SpriteFactory.Asteroid(Random.Range(0, 4)), pos);
                    o.tr.localScale = Vector3.one * 0.95f;
                    o.radius = 0.42f;
                    o.vel = new Vector2(Random.Range(-0.4f, 0.4f), -speed * 1.1f);
                    o.spin = Random.Range(-120f, 120f);
                    break;
                case ObstacleType.BigAsteroid:
                    o = NewObstacle(type, SpriteFactory.Asteroid(Random.Range(4, 8)), pos);
                    o.tr.localScale = Vector3.one * 1.8f;
                    o.radius = 0.8f;
                    o.vel = new Vector2(Random.Range(-0.25f, 0.25f), -speed * 0.85f);
                    o.spin = Random.Range(-50f, 50f);
                    break;
                case ObstacleType.Drone:
                    o = NewObstacle(type, SpriteFactory.Drone, pos);
                    o.tr.localScale = Vector3.one * 0.95f;
                    o.radius = 0.45f;
                    o.vel = new Vector2(0, -speed * 0.9f);
                    break;
                case ObstacleType.Mine:
                    o = NewObstacle(type, SpriteFactory.Mine, pos);
                    o.tr.localScale = Vector3.one * 1.05f;
                    o.radius = 0.45f;
                    o.vel = new Vector2(0, -speed * 0.75f);
                    o.spin = 60f;
                    break;
                default: // MetalWall
                    o = NewObstacle(ObstacleType.MetalWall, SpriteFactory.Wall, pos);
                    float w = Random.Range(1.4f, 2.6f + _cfg.level * 0.01f);
                    o.tr.localScale = new Vector3(w / 2f, 1f, 1f);
                    o.box = true;
                    o.half = new Vector2(w / 2f, 0.25f);
                    o.vel = new Vector2(0, -speed * 0.95f);
                    break;
            }
            o.baseColor = o.sr.color;
        }

        void SpawnLaserGate()
        {
            float gap = Mathf.Lerp(3.0f, 2.1f, (_cfg.level - 18) / 52f);
            float gapCenter = Random.Range(-_halfW + gap / 2f + 0.5f, _halfW - gap / 2f - 0.5f);
            float speed = _cfg.obstacleSpeed * 0.8f;
            float y = _halfH + 1.5f;
            // lado izquierdo
            float leftW = gapCenter - gap / 2f + _halfW;
            float rightW = _halfW - (gapCenter + gap / 2f);
            foreach (var (cx, w) in new[] { (-_halfW + leftW / 2f, leftW), (_halfW - rightW / 2f, rightW) })
            {
                if (w < 0.2f) continue;
                var o = NewObstacle(ObstacleType.LaserGate, SpriteFactory.Laser, new Vector2(cx, y));
                o.tr.localScale = new Vector3(w, 1.4f, 1f);
                o.box = true;
                o.half = new Vector2(w / 2f, 0.12f);
                o.vel = new Vector2(0, -speed);
                o.baseColor = Color.white;
            }
        }

        void UpdateObstacles(float dt)
        {
            Vector2 shipPos = _ship.position;
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                var o = _obstacles[i];
                o.phase += dt;
                if (o.type == ObstacleType.Drone) o.vel.x = Mathf.Sin(o.phase * 2.5f) * 2.2f;
                if (o.type == ObstacleType.Mine) o.vel.x = Mathf.Clamp(shipPos.x - o.tr.position.x, -1f, 1f) * 0.6f;
                if (o.type == ObstacleType.LaserGate) o.sr.color = new Color(1, 1, 1, 0.75f + Mathf.Sin(o.phase * 20f) * 0.25f);

                Vector2 p = (Vector2)o.tr.position + o.vel * dt;
                if (!o.box && Mathf.Abs(p.x) > _halfW - o.radius) { o.vel.x = -o.vel.x; p.x = Mathf.Clamp(p.x, -_halfW + o.radius, _halfW - o.radius); }
                o.tr.position = p;
                if (o.spin != 0) o.tr.Rotate(0, 0, o.spin * dt);

                if (o.flash > 0) { o.flash -= dt; o.sr.color = Color.Lerp(o.baseColor, Color.red, o.flash * 5f); }

                if (p.y < -_halfH - 2f) { RemoveObstacle(i, false); continue; }

                // Colisión con la nave
                if (!o.hitPlayer && Overlaps(o, shipPos, ShipRadius))
                {
                    o.hitPlayer = true;
                    HitPlayer(o);
                    if (State != RunState.Running) return;
                    if (o.destructible || o.type == ObstacleType.Mine) RemoveObstacle(i, true);
                }
            }
        }

        static bool Overlaps(Obstacle o, Vector2 c, float r)
        {
            Vector2 op = o.tr.position;
            if (!o.box) return (op - c).sqrMagnitude < (o.radius + r) * (o.radius + r);
            Vector2 closest = new Vector2(Mathf.Clamp(c.x, op.x - o.half.x, op.x + o.half.x), Mathf.Clamp(c.y, op.y - o.half.y, op.y + o.half.y));
            return (closest - c).sqrMagnitude < r * r;
        }

        void HitPlayer(Obstacle o)
        {
            if (_invuln > 0) return;
#if UNITY_EDITOR
            if (TrailerCheats.Invincible) return; // solo para grabar el video promocional
#endif
            if (TempShieldTime > 0)
            {
                TempShieldTime = 0f; // absorbe un choque y se gasta
                _invuln = 0.8f;
                AudioFX.Instance.Play(AudioFX.Sfx.Shield);
                return;
            }
            if (ShieldHits > 0)
            {
                ShieldHits--;
                _invuln = 0.8f;
                AudioFX.Instance.Play(AudioFX.Sfx.Shield);
                return;
            }
            // Indestructibles (muro, mina, láser): quitan un % fijo de la vida máxima (50 %, o 25 % desde el nivel 25).
            // Rompibles: daño según tipo, nivel y blindaje.
            float dmg = o.destructible
                ? LevelDatabase.BaseDamage(o.type) * _cfg.damageMultiplier * (1f - _stats.damageReduction)
                : _stats.maxHealth * GameConfig.IndestructibleDamageFor(Level);
            Health -= dmg;
            TookDamage = true;
            _invuln = 1f;
            _shake = 0.35f;
            AudioFX.Instance.Play(AudioFX.Sfx.Hit);
            AudioFX.Vibrate();
            UIManager.Instance.FlashDamage();
            if (Health <= 0)
            {
                Health = 0;
                Explode(_ship.position, _stats.color, 40, 1.6f);
                AudioFX.Instance.Play(AudioFX.Sfx.Explosion);
                _ship.gameObject.SetActive(false);
                State = RunState.Dying;
                _dyingTimer = 1.2f;
            }
        }

        /// <summary>Drones y asteroides destruidos suman a las misiones en el momento (cuentan aunque luego se pierda el nivel).</summary>
        static void CountForMissions(ObstacleType t)
        {
            if (t == ObstacleType.Drone) GameManager.Instance.MissionEvent(MissionType.DestroyDrones, 1);
            else if (t == ObstacleType.SmallAsteroid || t == ObstacleType.BigAsteroid) GameManager.Instance.MissionEvent(MissionType.DestroyAsteroids, 1);
        }

        void RemoveObstacle(int i, bool explode)
        {
            var o = _obstacles[i];
            if (explode)
            {
                Color c = o.type == ObstacleType.Drone ? new Color(1f, 0.3f, 0.3f) : o.type == ObstacleType.Mine ? new Color(1f, 0.5f, 0.2f) : new Color(0.7f, 0.6f, 0.5f);
                Explode(o.tr.position, c, o.type == ObstacleType.BigAsteroid ? 24 : 14, o.type == ObstacleType.BigAsteroid ? 1.3f : 1f);
                AudioFX.Instance.Play(AudioFX.Sfx.Explosion, 0.6f);
            }
            Destroy(o.tr.gameObject);
            _obstacles.RemoveAt(i);
        }

        // ---------------- PROYECTILES ----------------
        void SpawnProjectile(Vector2 pos, Vector2? velocity = null, int pierce = 0, bool plasma = false)
        {
            Vector2 vel = velocity ?? new Vector2(0, 16f);
            var go = new GameObject("Bullet");
            go.transform.SetParent(_world);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(vel.x, vel.y) * Mathf.Rad2Deg);
            go.transform.localScale = new Vector3(0.22f, 0.55f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Circle;
            sr.color = plasma ? new Color(1f, 0.6f, 1f) : new Color(0.5f, 1f, 1f);
            sr.sortingOrder = 5;
            var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(go.transform, false);
            glow.transform.localScale = Vector3.one * 3f;
            glow.sprite = SpriteFactory.Glow;
            glow.color = plasma ? new Color(1f, 0.3f, 0.9f, 0.65f) : new Color(0.3f, 0.8f, 1f, 0.6f);
            glow.sortingOrder = 4;
            _projectiles.Add(new Projectile { tr = go.transform, vel = vel, pierce = pierce });
        }

        void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var p = _projectiles[i];
                Vector2 pos = (Vector2)p.tr.position + p.vel * dt;
                p.tr.position = pos;
                bool remove = pos.y > _halfH + 1f || Mathf.Abs(pos.x) > _halfW + 1f;

                for (int j = _obstacles.Count - 1; j >= 0 && !remove; j--)
                {
                    var o = _obstacles[j];
                    if (!Overlaps(o, pos, 0.15f)) continue;
                    if (p.hits != null && p.hits.Contains(o)) continue;
                    // El plasma atraviesa obstáculos rompibles; contra los indestructibles se deshace
                    if (o.destructible && p.pierce > 0) { p.pierce--; (p.hits ??= new HashSet<Obstacle>()).Add(o); }
                    else remove = true;
                    if (o.destructible)
                    {
                        o.hp--;
                        o.flash = 0.2f;
                        if (o.hp <= 0)
                        {
                            Destroyed++;
                            KillPoints += LevelDatabase.PointsForDestroying(o.type);
                            CountForMissions(o.type);
                            RemoveObstacle(j, true);
                        }
                        else AudioFX.Instance.Play(AudioFX.Sfx.Hit, 0.4f);
                    }
                    else
                    {
                        // Indestructible: la bala rebota con chispas
                        Explode(pos, new Color(1f, 0.9f, 0.5f), 5, 0.5f);
                        AudioFX.Instance.Play(AudioFX.Sfx.Clang, 0.5f);
                    }
                }

                if (remove) { Destroy(p.tr.gameObject); _projectiles.RemoveAt(i); }
            }
        }

        // ---------------- MONEDAS Y MEJORAS TEMPORALES ----------------
        void SetupPickups(int level)
        {
            PickupsEnabled = level >= GameConfig.PickupsFromLevel;
            RunCoins = 0; RunCoinPickups = 0; TempShieldTime = 0; OverchargeTime = 0; MagnetTime = 0;
            TrialCannonTier = 0; TrialProductId = null; _trialPending = false;
            Weapon.ChargeRateMultiplier = 1f;
            if (!PickupsEnabled) return;
            float t = Mathf.InverseLerp(GameConfig.PickupsFromLevel, GameConfig.TotalLevels, level);
            int count = Mathf.RoundToInt(Mathf.Lerp(GameConfig.CoinsPerLevelMin, GameConfig.CoinsPerLevelMax, t));
            _coinInterval = Mathf.Max(2f, (_cfg.duration - 6f) / count);
            _coinTimer = 3f;
            _coinValue = Mathf.RoundToInt(Mathf.Lerp(GameConfig.CoinValueMin, GameConfig.CoinValueMax, t));
            _powerUpTimer = Random.Range(8f, 12f);
#if UNITY_EDITOR
            if (TrailerCheats.FrequentPowerUps) _powerUpTimer = 2f;
#endif

            // Prueba de un arma de pago: como mucho una por nivel y solo de las que no tiene
            bool tryTrial = level >= GameConfig.TrialsFromLevel && Random.value < GameConfig.TrialChancePerLevel;
#if UNITY_EDITOR
            if (TrailerCheats.ForceTrial) tryTrial = true;
#endif
            if (tryTrial)
            {
                var owned = GameManager.Instance.Data.ownedEquipment;
                var options = new List<string>();
                foreach (var pid in TrialProducts)
                {
                    var eq = UpgradeCatalog.EquipmentByProduct(pid);
                    if (eq != null && !owned.Contains(eq.id)) options.Add(pid);
                }
                if (options.Count > 0)
                {
                    _trialProduct = options[Random.Range(0, options.Count)];
                    _trialPending = true;
                    _trialAt = _cfg.duration * Random.Range(0.25f, 0.45f);
#if UNITY_EDITOR
                    if (TrailerCheats.ForceTrial) _trialAt = 4f;
#endif
                }
            }
        }

        void UpdatePickupSpawns(float dt)
        {
            if (_trialPending && _time >= _trialAt) { _trialPending = false; SpawnTrial(_trialProduct); }
            if (_time >= _cfg.duration - 3f) return; // no aparecen justo al final
            _coinTimer -= dt;
            if (_coinTimer <= 0)
            {
                SpawnPickup(true, PowerUpType.Shield);
                _coinTimer = _coinInterval * Random.Range(0.8f, 1.2f);
            }
            _powerUpTimer -= dt;
            if (_powerUpTimer <= 0)
            {
                SpawnPickup(false, PickPowerUp());
                _powerUpTimer = GameConfig.PowerUpInterval * Random.Range(0.85f, 1.15f);
#if UNITY_EDITOR
                if (TrailerCheats.FrequentPowerUps) _powerUpTimer = 6f;
#endif
            }
        }

        PowerUpType PickPowerUp()
        {
            // La reparación solo sale si la nave ha recibido daño
            bool hurt = Health < _stats.maxHealth * 0.85f;
            float r = Random.value * (hurt ? 100f : 80f);
            if (r < 30f) return PowerUpType.Shield;
            if (r < 55f) return PowerUpType.Overcharge;
            if (r < 80f) return PowerUpType.Magnet;
            return PowerUpType.Repair;
        }

        void SpawnPickup(bool coin, PowerUpType type)
        {
            var go = new GameObject(coin ? "Coin" : "PowerUp");
            go.transform.SetParent(_world);
            go.transform.position = new Vector3(Random.Range(-_halfW + 0.8f, _halfW - 0.8f), _halfH + 1f, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = coin ? SpriteFactory.Coin : SpriteFactory.PowerUp(type);
            sr.sortingOrder = 3;
            go.transform.localScale = Vector3.one * (coin ? 0.6f : 0.95f);
            _pickups.Add(new Pickup
            {
                tr = go.transform, coin = coin, type = type,
                vel = new Vector2(0, -_cfg.obstacleSpeed * (coin ? 0.6f : 0.5f)),
                radius = coin ? 0.3f : 0.45f, phase = Random.value * 6f
            });
        }

        void UpdatePickups(float dt)
        {
            Vector2 ship = _ship.position;
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var k = _pickups[i];
                k.phase += dt;
                Vector2 p = k.tr.position;
                if (k.coin && MagnetTime > 0 && (ship - p).sqrMagnitude < 4.5f * 4.5f)
                    p = Vector2.MoveTowards(p, ship, 11f * dt); // el imán atrae las monedas cercanas
                else p += k.vel * dt;
                k.tr.position = p;
                k.tr.localScale = k.coin
                    ? new Vector3(0.08f + 0.52f * Mathf.Abs(Mathf.Cos(k.phase * 3f)), 0.6f, 1f) // la moneda gira
                    : Vector3.one * (0.95f + Mathf.Sin(k.phase * 5f) * 0.06f);                 // la mejora late

                if (p.y < -_halfH - 1.5f) { Destroy(k.tr.gameObject); _pickups.RemoveAt(i); continue; }
                float rr = k.radius + ShipRadius;
                if ((p - ship).sqrMagnitude < rr * rr)
                {
                    Collect(k);
                    Destroy(k.tr.gameObject);
                    _pickups.RemoveAt(i);
                }
            }
        }

        void Collect(Pickup k)
        {
            Vector2 pos = k.tr.position;
            if (k.trial != null) { ApplyTrial(k.trial, pos); return; }
            if (k.coin)
            {
                RunCoins += _coinValue;
                RunCoinPickups++;
                AudioFX.Instance.Play(AudioFX.Sfx.Coin, 0.7f);
                Explode(pos, new Color(1f, 0.85f, 0.3f), 6, 0.5f);
                return;
            }
            AudioFX.Instance.Play(AudioFX.Sfx.PowerUp);
            Explode(pos, PowerUps.ColorOf(k.type), 12, 0.8f);
            switch (k.type)
            {
                case PowerUpType.Shield: TempShieldTime = GameConfig.PowerUpShieldSeconds; break;
                case PowerUpType.Overcharge: OverchargeTime = GameConfig.PowerUpOverchargeSeconds; break;
                case PowerUpType.Magnet: MagnetTime = GameConfig.PowerUpMagnetSeconds; break;
                default: Health = Mathf.Min(_stats.maxHealth, Health + _stats.maxHealth * GameConfig.PowerUpRepairPercent); break;
            }
            UIManager.Instance.Toast(L.T("toast.pu_fmt", PowerUps.Name(k.type)));
        }

        void UpdatePowerUpTimers(float dt)
        {
            if (TempShieldTime > 0) TempShieldTime = Mathf.Max(0f, TempShieldTime - dt);
            if (OverchargeTime > 0) OverchargeTime = Mathf.Max(0f, OverchargeTime - dt);
            if (MagnetTime > 0) MagnetTime = Mathf.Max(0f, MagnetTime - dt);
            Weapon.ChargeRateMultiplier = OverchargeTime > 0 ? OverchargeMultiplier() : 1f;
        }

        /// <summary>
        /// La Sobrecarga acelera la carga, pero respetando la regla del juego:
        /// la carga nunca dura menos que el tiempo de disparo.
        /// </summary>
        float OverchargeMultiplier()
        {
            float limit = Mathf.Max(1f, _stats.chargeTime / Mathf.Max(0.1f, _stats.fireWindow * 1.05f));
            return Mathf.Clamp(GameConfig.PowerUpOverchargeMultiplier, 1f, limit);
        }

        // ---------------- PRUEBA DE ARMAS DE PAGO ----------------
        void SpawnTrial(string productId)
        {
            var go = new GameObject("Trial");
            go.transform.SetParent(_world);
            go.transform.position = new Vector3(Random.Range(-_halfW + 1f, _halfW - 1f), _halfH + 1.2f, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Icon(IconPainter.KeyForProduct(productId));
            sr.sortingOrder = 3;
            var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(go.transform, false);
            glow.transform.localScale = Vector3.one * 2.4f;
            glow.sprite = SpriteFactory.Glow;
            glow.color = new Color(1f, 0.85f, 0.3f, 0.6f);
            glow.sortingOrder = 2;
            _pickups.Add(new Pickup { tr = go.transform, trial = productId, vel = new Vector2(0, -_cfg.obstacleSpeed * 0.45f), radius = 0.55f });
        }

        void ApplyTrial(string productId, Vector2 pos)
        {
            var eq = UpgradeCatalog.EquipmentByProduct(productId);
            if (eq == null) return;
            if (eq.cannonTier > 0) TrialCannonTier = Mathf.Max(TrialCannonTier, eq.cannonTier);
            if (eq.missiles && !MissilesEnabled) SetupMissiles(true);
            TrialProductId = productId;
            AudioFX.Instance.Play(AudioFX.Sfx.PowerUp);
            Explode(pos, new Color(1f, 0.85f, 0.3f), 18, 1f);
            var def = ProductCatalog.Get(productId);
            UIManager.Instance.Toast(L.T("toast.trial_fmt", def != null ? def.Title : eq.Name));
        }

        // ---------------- MISILES ----------------
        void SetupMissiles(bool enabled)
        {
            MissilesEnabled = enabled;
            MissilesInLoad = enabled ? GameConfig.MissilesPerLoad : 0;
            _missileReloadsLeft = enabled ? GameConfig.MissileLoadsPerLevel - 1 : 0;
            MissileReloadTime = 0f;
        }

        public void MissilePressed()
        {
            if (State != RunState.Running || !MissilesEnabled || MissilesInLoad <= 0) return;
            MissilesInLoad--;
            SpawnMissile();
            AudioFX.Instance.Play(AudioFX.Sfx.Missile);
            if (MissilesInLoad == 0 && _missileReloadsLeft > 0)
            {
                _missileReloadsLeft--;
                MissileReloadTime = GameConfig.MissileReloadSeconds;
            }
        }

        void SpawnMissile()
        {
            var go = new GameObject("Missile");
            go.transform.SetParent(_world);
            go.transform.position = _ship.position + Vector3.up * 0.6f;
            go.transform.localScale = new Vector3(0.18f, 0.5f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.White;
            sr.color = new Color(0.95f, 0.95f, 1f);
            sr.sortingOrder = 6;
            var flame = new GameObject("Flame").AddComponent<SpriteRenderer>();
            flame.transform.SetParent(go.transform, false);
            flame.transform.localPosition = new Vector3(0, -0.7f, 0);
            flame.transform.localScale = new Vector3(3f, 1.2f, 1f);
            flame.sprite = SpriteFactory.Glow;
            flame.color = new Color(1f, 0.55f, 0.15f, 0.85f);
            flame.sortingOrder = 5;
            _missiles.Add(new Missile { tr = go.transform, vel = new Vector2(0, 6f), target = FindMissileTarget(), life = 4f });
        }

        /// <summary>El obstáculo más cercano por delante de la nave (menos las barreras láser), sin repetir objetivo.</summary>
        Obstacle FindMissileTarget()
        {
            Vector2 ship = _ship.position;
            Obstacle best = null, bestTaken = null;
            float bestD = float.MaxValue, bestTakenD = float.MaxValue;
            foreach (var o in _obstacles)
            {
                if (o.type == ObstacleType.LaserGate || o.tr.position.y < ship.y - 0.5f || o.tr.position.y > _halfH + 0.5f) continue;
                float d = ((Vector2)o.tr.position - ship).sqrMagnitude;
                bool taken = _missiles.Exists(m => m.target == o);
                if (!taken && d < bestD) { best = o; bestD = d; }
                if (taken && d < bestTakenD) { bestTaken = o; bestTakenD = d; }
            }
            return best ?? bestTaken;
        }

        void UpdateMissiles(float dt)
        {
            if (MissileReloadTime > 0f)
            {
                MissileReloadTime -= dt;
                if (MissileReloadTime <= 0f)
                {
                    MissileReloadTime = 0f;
                    MissilesInLoad = GameConfig.MissilesPerLoad;
                    AudioFX.Instance.Play(AudioFX.Sfx.Ready, 0.6f);
                }
            }

            for (int i = _missiles.Count - 1; i >= 0; i--)
            {
                var m = _missiles[i];
                m.life -= dt;
                if (m.target == null || !_obstacles.Contains(m.target)) m.target = FindMissileTarget();
                Vector2 pos = m.tr.position;
                float speed = Mathf.Min(15f, m.vel.magnitude + 14f * dt);
                Vector2 desired = m.target != null ? ((Vector2)m.target.tr.position - pos).normalized : Vector2.up;
                m.vel = (Vector2)Vector3.RotateTowards(m.vel.normalized, desired, 7f * dt, 0f) * speed;
                pos += m.vel * dt;
                m.tr.position = pos;
                m.tr.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(m.vel.x, m.vel.y) * Mathf.Rad2Deg);

                bool remove = m.life <= 0f || Mathf.Abs(pos.y) > _halfH + 2f || Mathf.Abs(pos.x) > _halfW + 2f;
                for (int j = _obstacles.Count - 1; j >= 0 && !remove; j--)
                {
                    var o = _obstacles[j];
                    if (o.type == ObstacleType.LaserGate || !Overlaps(o, pos, 0.3f)) continue;
                    // El misil destruye cualquier obstáculo de un golpe, incluidos muros y minas
                    Destroyed++;
                    KillPoints += o.destructible ? LevelDatabase.PointsForDestroying(o.type) : 40;
                    CountForMissions(o.type);
                    Explode(o.tr.position, new Color(1f, 0.6f, 0.2f), 26, 1.5f);
                    _shake = Mathf.Max(_shake, 0.2f);
                    RemoveObstacle(j, true);
                    remove = true;
                }
                if (remove) { Destroy(m.tr.gameObject); _missiles.RemoveAt(i); }
            }
        }

        // ---------------- EFECTOS ----------------
        void Explode(Vector2 pos, Color color, int count, float power)
        {
            for (int i = 0; i < count; i++)
            {
                Particle p;
                if (_particlePool.Count > 0) { p = _particlePool.Pop(); p.tr.gameObject.SetActive(true); }
                else
                {
                    var go = new GameObject("Particle");
                    go.transform.SetParent(_world);
                    p = new Particle { tr = go.transform, sr = go.AddComponent<SpriteRenderer>() };
                    p.sr.sprite = SpriteFactory.White;
                    p.sr.sortingOrder = 15;
                }
                p.tr.position = pos;
                p.tr.localScale = Vector3.one * Random.Range(0.08f, 0.2f) * power;
                p.vel = Random.insideUnitCircle * Random.Range(2f, 7f) * power;
                p.maxLife = p.life = Random.Range(0.35f, 0.8f);
                p.sr.color = Color.Lerp(color, Color.white, Random.value * 0.4f);
                _particles.Add(p);
            }
        }

        void UpdateParticles(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.life -= dt;
                if (p.life <= 0)
                {
                    p.tr.gameObject.SetActive(false);
                    _particlePool.Push(p);
                    _particles.RemoveAt(i);
                    continue;
                }
                p.tr.position += (Vector3)(p.vel * dt);
                p.vel *= 1f - 2f * dt;
                var c = p.sr.color; c.a = p.life / p.maxLife; p.sr.color = c;
            }
        }

        void UpdateShake(float dt)
        {
            if (_shake <= 0) { _cam.transform.position = new Vector3(0, 0, -10); return; }
            _shake -= dt;
            _cam.transform.position = new Vector3(Random.Range(-1f, 1f) * _shake * 0.5f, Random.Range(-1f, 1f) * _shake * 0.5f, -10);
        }
    }
}
