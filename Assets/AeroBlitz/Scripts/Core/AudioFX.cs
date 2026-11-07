using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>Efectos de sonido sintetizados en tiempo real (no requiere archivos de audio).</summary>
    public class AudioFX : MonoBehaviour
    {
        public enum Sfx { Shoot, Explosion, Hit, Clang, Ready, Win, Lose, Click, Upgrade, Shield, Coin, PowerUp, Missile }

        public static AudioFX Instance { get; private set; }
        AudioSource _src;
        readonly Dictionary<Sfx, AudioClip> _clips = new Dictionary<Sfx, AudioClip>();
        const int Rate = 22050;

        void Awake()
        {
            Instance = this;
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _clips[Sfx.Shoot] = Make(0.12f, (t, p) => Square(Mathf.Lerp(900, 300, p) * t) * (1 - p) * 0.35f);
            _clips[Sfx.Explosion] = Make(0.45f, (t, p) => Noise() * Mathf.Pow(1 - p, 2) * 0.6f);
            _clips[Sfx.Hit] = Make(0.25f, (t, p) => (Noise() * 0.5f + Square(120 * t) * 0.5f) * (1 - p) * 0.6f);
            _clips[Sfx.Clang] = Make(0.15f, (t, p) => Mathf.Sin(2 * Mathf.PI * 1400 * t) * (1 - p) * 0.3f);
            _clips[Sfx.Ready] = Make(0.25f, (t, p) => Mathf.Sin(2 * Mathf.PI * (p < 0.5f ? 660 : 990) * t) * 0.3f * (1 - p));
            _clips[Sfx.Win] = Make(0.7f, (t, p) => Mathf.Sin(2 * Mathf.PI * (p < 0.33f ? 523 : p < 0.66f ? 659 : 784) * t) * 0.3f * (1 - p * 0.5f));
            _clips[Sfx.Lose] = Make(0.7f, (t, p) => Square(Mathf.Lerp(300, 80, p) * t) * 0.25f * (1 - p));
            _clips[Sfx.Click] = Make(0.05f, (t, p) => Mathf.Sin(2 * Mathf.PI * 1200 * t) * (1 - p) * 0.25f);
            _clips[Sfx.Upgrade] = Make(0.35f, (t, p) => Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(400, 1200, p) * t) * 0.3f * (1 - p));
            _clips[Sfx.Coin] = Make(0.14f, (t, p) => Mathf.Sin(2 * Mathf.PI * (p < 0.4f ? 1320 : 1760) * t) * 0.25f * (1 - p));
            _clips[Sfx.PowerUp] = Make(0.42f, (t, p) => (Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(600, 1500, p) * t) + 0.4f * Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(900, 2250, p) * t)) * 0.22f * (1 - p * 0.6f));
            _clips[Sfx.Missile] = Make(0.4f, (t, p) => (Noise() * 0.55f + Square(Mathf.Lerp(220, 70, p) * t) * 0.45f) * (1 - p) * 0.4f);
            _clips[Sfx.Shield] = Make(0.3f, (t, p) => Mathf.Sin(2 * Mathf.PI * (500 + 200 * Mathf.Sin(p * 40)) * t) * 0.3f * (1 - p));
        }

        static float Square(float phase) => Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f;
        static float Noise() => Random.value * 2f - 1f;

        static AudioClip Make(float seconds, System.Func<float, float, float> fn)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = fn(i / (float)Rate, i / (float)n);
            var clip = AudioClip.Create("sfx", n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void Play(Sfx s, float volume = 1f)
        {
            var d = GameManager.Instance != null ? GameManager.Instance.Data : null;
            if (d != null && !d.soundOn) return;
            _src.PlayOneShot(_clips[s], volume);
        }

        public static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            var d = GameManager.Instance != null ? GameManager.Instance.Data : null;
            if (d != null && d.vibrationOn) Handheld.Vibrate();
#endif
        }
    }
}
