using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Arma con barra de carga:
    ///  1) Cargando: la barra se llena durante chargeTime.
    ///  2) Lista: la barra llena espera al primer disparo.
    ///  3) Disparando: tienes fireWindow segundos y un máximo de N disparos (según el nivel de la nave).
    /// La carga siempre dura más que la ventana de disparo (ver ShipStats).
    /// </summary>
    public class WeaponSystem
    {
        public enum State { Charging, Ready, Firing }

        public State Current { get; private set; }
        public float Charge01 { get; private set; }
        public int ShotsLeft { get; private set; }
        public int MaxShots { get; private set; }
        /// <summary>Velocidad de carga (1 = normal). La mejora Sobrecarga la sube durante unos segundos.</summary>
        public float ChargeRateMultiplier { get; set; } = 1f;

        float _chargeTime, _fireWindow, _windowLeft, _cooldown;
        const float ShotCooldown = 0.16f;

        public void Setup(ShipStats stats)
        {
            _chargeTime = Mathf.Max(1f, stats.chargeTime);
            _fireWindow = stats.fireWindow;
            MaxShots = stats.shots;
            // Se empieza con la barra a la mitad para que el jugador aprenda rápido la mecánica
            Charge01 = 0.5f;
            Current = State.Charging;
            ShotsLeft = 0;
        }

        /// <summary>Valor a mostrar en la barra (0..1).</summary>
        public float Bar01 => Current == State.Charging ? Charge01 : Current == State.Ready ? 1f : _windowLeft / _fireWindow;

        public void Tick(float dt)
        {
            _cooldown -= dt;
            switch (Current)
            {
                case State.Charging:
                    Charge01 += dt * ChargeRateMultiplier / _chargeTime;
                    if (Charge01 >= 1f)
                    {
                        Charge01 = 1f;
                        Current = State.Ready;
                        AudioFX.Instance.Play(AudioFX.Sfx.Ready, 0.6f);
                    }
                    break;
                case State.Firing:
                    _windowLeft -= dt;
                    if (_windowLeft <= 0f || ShotsLeft <= 0) StartCharging();
                    break;
            }
        }

        void StartCharging()
        {
            Current = State.Charging;
            Charge01 = 0f;
            ShotsLeft = 0;
        }

        /// <summary>Devuelve true si se debe crear un proyectil.</summary>
        public bool TryFire()
        {
            if (_cooldown > 0f) return false;
            if (Current == State.Ready)
            {
                Current = State.Firing;
                _windowLeft = _fireWindow;
                ShotsLeft = MaxShots;
            }
            if (Current != State.Firing || ShotsLeft <= 0) return false;
            ShotsLeft--;
            _cooldown = ShotCooldown;
            return true;
        }
    }
}
