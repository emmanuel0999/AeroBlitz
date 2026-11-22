using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Motor de jefes (niveles 10, 20 ... 70). Todos los jefes usan este mismo código: sus datos están en BossCatalog.
    ///  - Entra desde arriba, se mueve de lado a lado y ataca con los patrones de su fase actual.
    ///  - Tiene 3 fases (100-66 %, 66-33 %, 33-0 %): la última es el modo furia.
    ///  - Los disparos del jefe solo se esquivan; las balas y misiles del jugador solo dañan al jefe.
    ///  - Su vida se calcula según el arma de la nave del jugador, para que la pelea dure lo mismo con cualquier nave.
    /// </summary>
    public partial class GameplayController
    {
        class BossUnit
        {
            public BossDefinition def;
            public Transform tr;
            public SpriteRenderer hull, lights, core;
            public float hp, maxHp;
            public int phase = -1;
            public float t, atkTimer, summonTimer, flash, enter, deadT, nextBoom;
            public bool dead;
            public bool ready;     // sprites listos (se dibujan en segundo plano)
            public float waitT;
            public int patIdx;
            public float x;
        }

        class ScheduledShot { public float time; public System.Action action; }

        BossUnit _boss;
        bool _bossFinished;
        readonly List<ScheduledShot> _bossQueue = new List<ScheduledShot>();

        const float BossEnterSeconds = 2.8f;
        const float BossDeathSeconds = 2.8f;

        /// <summary>Hay un jefe vivo (incluye la entrada): el arma carga más rápido.</summary>
        public bool BossActive => _boss != null && !_boss.dead;
        /// <summary>El jefe ya puede recibir daño.</summary>
        public bool BossTargetable => _boss != null && !_boss.dead && _boss.ready && _boss.enter >= 1f;
        public bool BossFinished => _bossFinished;
        /// <summary>El nivel actual es de jefe (la barra de arriba muestra su vida).</summary>
        public bool IsBossLevel => _cfg != null && _cfg.boss;
        public float BossHp01 => _boss == null ? 0f : Mathf.Clamp01(_boss.hp / _boss.maxHp);
        public string BossName => _boss == null ? "" : _boss.def.Name;
        public int BossPhaseIndex => _boss == null ? 0 : Mathf.Max(0, _boss.phase);
        Vector2 BossPosition => _boss.tr.position;
        float BossY => _halfH - 4.3f;

        // ---------------- PREPARACIÓN ----------------
        void BossSetup(int level)
        {
            BossClear();
            if (_cfg == null || !_cfg.boss) return;
            var def = BossCatalog.ForLevel(level);
            if (def == null) return;
            SpriteFactory.PrepareBoss(def); // normalmente ya empezó durante el nivel anterior

            var go = new GameObject("Boss_" + def.id);
            go.transform.SetParent(_world);
            go.transform.position = new Vector3(0, _halfH + def.width * 0.5f + 0.5f, 0);

            var hull = go.AddComponent<SpriteRenderer>();
            hull.enabled = false;
            hull.color = def.hull;
            hull.sortingOrder = 1;

            var lights = new GameObject("Lights").AddComponent<SpriteRenderer>();
            lights.transform.SetParent(go.transform, false);
            lights.enabled = false;
            lights.sortingOrder = 2;

            var core = new GameObject("Core").AddComponent<SpriteRenderer>();
            core.transform.SetParent(go.transform, false);
            core.transform.localPosition = new Vector3(0, def.coreY * def.width * 0.5f * 0.92f, 0);
            core.transform.localScale = Vector3.one * 1.0f;
            core.sprite = SpriteFactory.Glow;
            core.color = def.accent;
            core.sortingOrder = 3;
            core.enabled = false;

            float maxHp = BossMaxHp(def);
            _boss = new BossUnit { def = def, tr = go.transform, hull = hull, lights = lights, core = core, hp = maxHp, maxHp = maxHp };
            _bossFinished = false;
            _bossQueue.Clear();
        }

        void BossClear()
        {
            if (_boss != null && _boss.tr != null) Destroy(_boss.tr.gameObject);
            _boss = null;
            _bossFinished = false;
            _bossQueue.Clear();
        }

        /// <summary>
        /// Vida del jefe: lo que haría tu arma durante el tiempo objetivo de la pelea (a la mitad de puntería).
        /// Así una nave con más munición o cañón mejor vence antes, pero ninguna se queda sin poder vencerlo.
        /// </summary>
        float BossMaxHp(BossDefinition d)
        {
            float cycle = _stats.chargeTime / GameConfig.BossChargeBoost + Mathf.Min(_stats.fireWindow, _stats.shots * 0.16f + 0.3f);
            float perShot = CannonTier == 0 ? 1f : CannonTier == 1 ? 2f : CannonTier == 2 ? 2.2f : 3f;
            float dps = _stats.shots * perShot / Mathf.Max(0.5f, cycle);
            return Mathf.Max(12f, Mathf.Round(dps * d.fightSeconds * 0.5f));
        }

        // ---------------- ACTUALIZACIÓN ----------------
        void BossTick(float dt)
        {
            var b = _boss;
            if (b == null) return;
            b.t += dt;
            if (b.flash > 0) b.flash -= dt;

            if (b.dead) { BossDeathTick(dt); return; }

            // Espera a que el dibujo en segundo plano termine (suele estar listo; si no, tarda un par de segundos)
            if (!b.ready)
            {
                b.waitT += dt;
                if (!SpriteFactory.TryFinishBoss(b.def)) return;
                b.hull.sprite = SpriteFactory.BossHull(b.def);
                b.lights.sprite = SpriteFactory.BossLights(b.def);
                b.hull.enabled = b.lights.enabled = b.core.enabled = true;
                b.ready = true;
                b.t = 0f;
            }

            // Entrada desde arriba
            if (b.enter < 1f)
            {
                b.enter = Mathf.Min(1f, b.enter + dt / BossEnterSeconds);
                float e = 1f - Mathf.Pow(1f - b.enter, 3f);
                float startY = _halfH + b.def.width * 0.5f + 0.5f;
                b.tr.position = new Vector3(0, Mathf.Lerp(startY, BossY, e), 0);
                if (b.enter >= 1f) BossStartPhase(0);
                BossVisuals();
                return;
            }

            // Fase según la vida que le queda
            float hp01 = b.hp / b.maxHp;
            int want = hp01 > 0.66f ? 0 : hp01 > 0.33f ? 1 : 2;
            if (want > b.phase) BossStartPhase(want);
            var ph = b.def.phases[b.phase];

            // Movimiento de lado a lado
            float range = Mathf.Min(b.def.moveRange, _halfW - b.def.width * 0.4f); // el jefe entero se queda dentro de la pantalla
            b.x = Mathf.Sin(b.t * b.def.moveSpeed * ph.moveMul * 0.55f) * Mathf.Max(0.5f, range);
            b.tr.position = new Vector3(b.x, BossY + Mathf.Sin(b.t * 1.3f) * 0.1f, 0);

            // Ataques
            b.atkTimer -= dt;
            if (b.atkTimer <= 0f)
            {
                BossLaunch(ph.patterns[b.patIdx % ph.patterns.Length], ph);
                b.patIdx++;
                b.atkTimer = ph.interval;
            }
            if (ph.summonCount > 0)
            {
                b.summonTimer -= dt;
                if (b.summonTimer <= 0f) { BossSummon(ph); b.summonTimer = ph.summonEvery; }
            }

            // Disparos programados (ráfagas, barridos, lluvias)
            for (int i = _bossQueue.Count - 1; i >= 0; i--)
            {
                var q = _bossQueue[i];
                q.time -= dt;
                if (q.time <= 0f)
                {
                    _bossQueue.RemoveAt(i);
                    if (_boss != null && !_boss.dead) q.action();
                }
            }
            BossVisuals();
        }

        void BossVisuals()
        {
            var b = _boss;
            if (b == null) return;
            float fury = b.phase >= 2 ? 1f : 0f;
            float flash = Mathf.Clamp01(b.flash * 7f);
            b.hull.color = Color.Lerp(b.def.hull, Color.white, flash);
            var acc = Color.Lerp(b.def.accent, new Color(1f, 0.2f, 0.15f), fury * 0.6f);
            float pulse = 0.75f + Mathf.Sin(b.t * (4f + fury * 6f)) * 0.25f;
            b.lights.color = new Color(1f, 1f, 1f, 0.82f + pulse * 0.18f); // el color ya viene en el dibujo
            b.core.color = new Color(acc.r, acc.g, acc.b, 0.25f + pulse * 0.25f + fury * 0.2f);
            b.core.transform.localScale = Vector3.one * (1.0f + Mathf.Sin(b.t * (5f + fury * 7f)) * 0.15f);
        }

        void BossStartPhase(int idx)
        {
            var b = _boss;
            b.phase = idx;
            var ph = b.def.phases[idx];
            b.atkTimer = idx == 0 ? 1.0f : 1.4f;
            b.summonTimer = ph.summonEvery * 0.6f;
            b.patIdx = 0;
            if (idx == 0)
            {
                UIManager.Instance.Toast(L.T("boss.warning_fmt", b.def.Name));
                _shake = Mathf.Max(_shake, 0.3f);
                return;
            }
            // Cambio de fase: se despejan los disparos para que el cambio sea justo y se avisa con fuerza
            _bossQueue.Clear();
            for (int i = _obstacles.Count - 1; i >= 0; i--)
                if (_obstacles[i].type == ObstacleType.BossShot) RemoveObstacle(i, false);
            _shake = Mathf.Max(_shake, 0.6f);
            Explode(b.tr.position, b.def.accent, 40, 1.6f);
            AudioFX.Instance.Play(AudioFX.Sfx.Explosion, 0.8f);
            AudioFX.Vibrate();
            UIManager.Instance.Toast(L.T(idx >= 2 ? "boss.fury" : "boss.phase_fmt", idx + 1));
        }

        // ---------------- ATAQUES ----------------
        Vector2 BossMuzzle() => (Vector2)_boss.tr.position + Vector2.down * (_boss.def.hit.y * 0.55f);

        static Vector2 DirDown(float angleDeg)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(a), -Mathf.Cos(a));
        }

        void BossQueueShot(float delay, System.Action a) => _bossQueue.Add(new ScheduledShot { time = delay, action = a });

        void BossLaunch(BossPattern pattern, BossPhase ph)
        {
            var b = _boss;
            float sp = ph.shotSpeed * (1f + (Level - 10) * 0.002f);
            int phase = b.phase;
            Vector2 from = BossMuzzle();

            switch (pattern)
            {
                case BossPattern.Aimed:
                    for (int k = 0; k < 3; k++)
                        BossQueueShot(k * 0.22f, () =>
                        {
                            Vector2 m = BossMuzzle();
                            Vector2 dir = ((Vector2)_ship.position - m).normalized;
                            SpawnBossShot(m, dir * sp * 1.1f);
                        });
                    break;

                case BossPattern.Fan:
                {
                    int n = 5 + phase * 2;
                    float spread = 56f + phase * 8f;
                    for (int i = 0; i < n; i++)
                        SpawnBossShot(from, DirDown(Mathf.Lerp(-spread / 2f, spread / 2f, i / (float)(n - 1))) * sp);
                    break;
                }

                case BossPattern.Spiral:
                {
                    int n = 14 + phase * 2;
                    for (int k = 0; k < n; k++)
                    {
                        int kk = k;
                        BossQueueShot(k * 0.1f, () => SpawnBossShot(BossMuzzle(), DirDown(Mathf.Sin(kk * 0.55f) * 58f) * sp * 0.95f));
                    }
                    break;
                }

                case BossPattern.Rain:
                {
                    int n = 7 + phase;
                    for (int k = 0; k < n; k++)
                        BossQueueShot(k * 0.17f, () => SpawnBossShot(new Vector2(Random.Range(-_halfW + 0.5f, _halfW - 0.5f), _halfH + 0.4f), Vector2.down * sp * 1.1f));
                    break;
                }

                case BossPattern.Ring:
                {
                    int n = 14 + phase * 2;
                    float off = Random.Range(0f, 360f);
                    for (int i = 0; i < n; i++)
                    {
                        float a = (off + i * 360f / n) * Mathf.Deg2Rad;
                        SpawnBossShot(from, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * sp * 0.85f);
                    }
                    break;
                }

                case BossPattern.GapWall:
                {
                    // Fila de disparos de lado a lado con un hueco de 3 espacios por donde hay que pasar
                    float step = 0.95f;
                    int slots = Mathf.CeilToInt(2f * (_halfW - 0.3f) / step);
                    int gap = Random.Range(1, Mathf.Max(2, slots - 3));
                    float startX = -(slots - 1) * step / 2f;
                    for (int i = 0; i < slots; i++)
                    {
                        if (i >= gap && i < gap + 3) continue;
                        SpawnBossShot(new Vector2(startX + i * step, from.y), Vector2.down * sp * 0.8f);
                    }
                    break;
                }
            }
            if (pattern != BossPattern.Rain) AudioFX.Instance.Play(AudioFX.Sfx.Missile, 0.3f);
        }

        void SpawnBossShot(Vector2 pos, Vector2 vel)
        {
            var o = NewObstacle(ObstacleType.BossShot, _boss != null ? SpriteFactory.BossShot(_boss.def) : SpriteFactory.Circle, pos);
            o.radius = 0.24f;
            o.baseScale = 0.5f;
            o.tr.localScale = Vector3.one * o.baseScale;
            o.vel = vel;
            var acc = _boss != null ? _boss.def.accent : Color.red;
            o.sr.sortingOrder = 4;
            o.sr.color = Color.white; // los colores ya vienen en el sprite
            o.baseColor = o.sr.color;
            var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(o.tr, false);
            glow.transform.localScale = Vector3.one * 3.4f;
            glow.sprite = SpriteFactory.Glow;
            glow.color = new Color(acc.r, acc.g, acc.b, 0.55f);
            glow.sortingOrder = 3;
        }

        /// <summary>Subordinados: drones y minas que salen del jefe (se pueden destruir como siempre).</summary>
        void BossSummon(BossPhase ph)
        {
            for (int i = 0; i < ph.summonCount; i++)
            {
                var kind = ph.summon == SummonKind.Mixed ? (i % 2 == 0 ? SummonKind.Drones : SummonKind.Mines) : ph.summon;
                float x = Mathf.Clamp(_boss.x + Random.Range(-1.6f, 1.6f), -_halfW + 0.8f, _halfW - 0.8f);
                SpawnObstacle(kind == SummonKind.Drones ? ObstacleType.Drone : ObstacleType.Mine, x, _boss.tr.position.y - 0.7f - i * 0.5f);
            }
            AudioFX.Instance.Play(AudioFX.Sfx.Ready, 0.5f);
        }

        // ---------------- DAÑO AL JEFE ----------------
        bool BossOverlaps(Vector2 p, float grow)
        {
            var c = (Vector2)_boss.tr.position;
            float dx = (p.x - c.x) / (_boss.def.hit.x + grow);
            float dy = (p.y - c.y) / (_boss.def.hit.y + grow);
            return dx * dx + dy * dy < 1f;
        }

        bool BossHitByProjectile(Projectile p, Vector2 pos)
        {
            if (!BossTargetable || !BossOverlaps(pos, 0.1f)) return false;
            BossDamage(1f, pos);
            return true; // la bala se gasta (el plasma no atraviesa jefes)
        }

        bool BossHitByMissile(Vector2 pos)
        {
            if (!BossTargetable || !BossOverlaps(pos, 0.25f)) return false;
            BossDamage(_boss.maxHp * GameConfig.BossMissileDamage01, pos);
            Explode(pos, new Color(1f, 0.6f, 0.2f), 26, 1.5f);
            _shake = Mathf.Max(_shake, 0.2f);
            return true;
        }

        /// <summary>La Llamarada del Fénix también hiere al jefe.</summary>
        void BossFlareHit()
        {
            if (!BossTargetable) return;
            BossDamage(_boss.maxHp * 0.06f, _boss.tr.position);
        }

        void BossDamage(float amount, Vector2 at)
        {
            var b = _boss;
            if (b == null || b.dead) return;
            b.hp -= amount;
            b.flash = 0.12f;
            Explode(at, b.def.accent, 5, 0.6f);
            AudioFX.Instance.Play(AudioFX.Sfx.Clang, 0.35f);
            if (b.hp <= 0f) BossDie();
        }

        void BossDie()
        {
            var b = _boss;
            b.hp = 0f;
            b.dead = true;
            b.deadT = 0f;
            b.nextBoom = 0f;
            _bossQueue.Clear();
            _invuln = 99f; // la nave no recibe daño mientras explota el jefe
            // Se despeja todo: disparos y subordinados explotan con él
            for (int i = _obstacles.Count - 1; i >= 0; i--) RemoveObstacle(i, _obstacles[i].type != ObstacleType.BossShot);
            _shake = 1f;
            AudioFX.Vibrate();
            AudioFX.Instance.Play(AudioFX.Sfx.Explosion);
            UIManager.Instance.Toast(L.T("boss.defeated"));
        }

        void BossDeathTick(float dt)
        {
            var b = _boss;
            b.deadT += dt;
            b.nextBoom -= dt;
            if (b.nextBoom <= 0f)
            {
                b.nextBoom = 0.12f;
                Vector2 c = b.tr.position;
                var at = c + new Vector2(Random.Range(-b.def.hit.x, b.def.hit.x), Random.Range(-b.def.hit.y, b.def.hit.y));
                Explode(at, Random.value < 0.5f ? b.def.accent : new Color(1f, 0.7f, 0.25f), 22, 1.4f);
                AudioFX.Instance.Play(AudioFX.Sfx.Explosion, 0.5f);
                _shake = Mathf.Max(_shake, 0.35f);
            }
            float k = Mathf.Clamp01(b.deadT / BossDeathSeconds);
            var hc = b.hull.color; hc.a = 1f - k * k; b.hull.color = hc;
            var lc = b.lights.color; lc.a = (1f - k) * 0.8f; b.lights.color = lc;
            b.core.transform.localScale = Vector3.one * (1.6f + k * 5f);
            var cc = b.core.color; cc.a = Mathf.Lerp(0.9f, 0f, k); b.core.color = cc;
            b.tr.position += (Vector3)(Vector2.down * dt * 0.25f);
            if (b.deadT >= BossDeathSeconds)
            {
                Explode(b.tr.position, Color.white, 60, 2.2f);
                _bossFinished = true;
            }
        }
    }
}
