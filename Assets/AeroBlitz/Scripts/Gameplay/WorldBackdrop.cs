using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Fondo del nivel: estrellas en varias capas, nubes de color y un planeta lejano según el mundo.
    /// Todo va por detrás del juego y con poca opacidad para no tapar los obstáculos.
    /// </summary>
    public class WorldBackdrop : MonoBehaviour
    {
        class Layer
        {
            public Transform tr;
            public SpriteRenderer sr;
            public float speed;
            public float alpha;
        }

        readonly List<Layer> _stars = new List<Layer>();
        readonly List<Layer> _nebulas = new List<Layer>();
        Layer _planet;
        Camera _cam;
        int _world = -1;

        const float Wrap = 9.5f;
        const float NebulaWrap = 15f;

        public void Build(Camera cam)
        {
            _cam = cam;
            for (int i = 0; i < 4; i++)
            {
                var n = NewLayer("Nebula", SpriteFactory.Glow, -30 + i);
                n.tr.localScale = Vector3.one * Random.Range(9f, 15f);
                n.tr.position = new Vector3(Random.Range(-6f, 6f), Random.Range(-NebulaWrap, NebulaWrap), 0);
                n.speed = Random.Range(0.12f, 0.3f);
                _nebulas.Add(n);
            }

            _planet = NewLayer("Planet", SpriteFactory.Planet, -25);
            _planet.speed = 0.08f;
            PlacePlanet(Random.Range(-4f, 8f));

            for (int i = 0; i < 70; i++)
            {
                float depth = Random.value;
                var s = NewLayer("Star", SpriteFactory.Circle, -10);
                s.tr.localScale = Vector3.one * Mathf.Lerp(0.03f, 0.12f, depth);
                s.tr.position = new Vector3(Random.Range(-9f, 9f), Random.Range(-9f, 9f), 0);
                s.speed = Mathf.Lerp(0.5f, 3f, depth);
                s.alpha = Mathf.Lerp(0.25f, 0.9f, depth);
                _stars.Add(s);
            }
        }

        Layer NewLayer(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return new Layer { tr = go.transform, sr = sr };
        }

        void PlacePlanet(float y)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            _planet.tr.position = new Vector3(side * Random.Range(2.5f, 4.5f), y, 0);
            _planet.tr.localScale = Vector3.one * Random.Range(2.2f, 3.6f);
        }

        /// <summary>Aplica los colores del mundo al que pertenece el nivel.</summary>
        public void ApplyForLevel(int level) => Apply(WorldCatalog.WorldForLevel(level));

        public void Apply(int world)
        {
            if (world == _world) return;
            _world = world;
            var w = WorldCatalog.Worlds[world];
            if (_cam != null) _cam.backgroundColor = w.background;

            foreach (var s in _stars)
                s.sr.color = new Color(w.stars.r, w.stars.g, w.stars.b, s.alpha);

            for (int i = 0; i < _nebulas.Count; i++)
            {
                var c = i % 2 == 0 ? w.nebulaA : w.nebulaB;
                _nebulas[i].sr.color = new Color(c.r, c.g, c.b, w.nebulaAlpha * (i < 2 ? 1f : 0.7f));
            }

            _planet.sr.enabled = w.planet;
            _planet.sr.color = new Color(w.planetColor.r, w.planetColor.g, w.planetColor.b, 0.32f);
        }

        /// <summary>Desplaza el fondo hacia abajo. speedBoost acompaña a la velocidad del nivel.</summary>
        public void Tick(float dt, float speedBoost, float mult)
        {
            foreach (var s in _stars)
            {
                var p = s.tr.position;
                p.y -= s.speed * speedBoost * mult * dt;
                if (p.y < -Wrap) { p.y = Wrap; p.x = Random.Range(-9f, 9f); }
                s.tr.position = p;
            }

            // Las nubes y el planeta están "más lejos": se mueven despacio y no aceleran con el nivel
            foreach (var n in _nebulas)
            {
                var p = n.tr.position;
                p.y -= n.speed * mult * dt;
                if (p.y < -NebulaWrap) { p.y = NebulaWrap; p.x = Random.Range(-6f, 6f); }
                n.tr.position = p;
            }

            var pp = _planet.tr.position;
            pp.y -= _planet.speed * mult * dt;
            _planet.tr.position = pp;
            if (pp.y < -12f) PlacePlanet(12f);
        }
    }
}
