using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    public enum PriceType { Free, Coins, RealMoney }

    public class ShipDefinition
    {
        public string id;
        public string Name => L.T("ship." + id);
        public string Description => L.T("ship." + id + ".desc");
        public PriceType priceType;
        public int coinPrice;
        public string productId;      // para naves de pago real
        public float maxHealth;
        public float speed;
        public int shotsPerCharge;
        public float chargeTime;      // segundos para llenar la barra
        public float fireWindow;      // segundos que dura la barra llena (siempre < chargeTime)
        public int tier;              // 1..3, define el "nivel" base de la nave
        public Vector2[] shape;       // polígono (espacio -1..1) para generar el sprite
        public Color accent;
    }

    public static class ShipCatalog
    {
        public const string StarterShipId = "falcon";

        public static readonly List<ShipDefinition> All = new List<ShipDefinition>
        {
            new ShipDefinition {
                id = "falcon",
                priceType = PriceType.Free, maxHealth = 100, speed = 6f, shotsPerCharge = 3,
                chargeTime = 9f, fireWindow = 3f, tier = 1, accent = new Color(0.3f, 0.8f, 1f),
                shape = new[] { new Vector2(0,1), new Vector2(0.35f,0.1f), new Vector2(0.9f,-0.5f), new Vector2(0.35f,-0.35f), new Vector2(0.25f,-0.8f), new Vector2(-0.25f,-0.8f), new Vector2(-0.35f,-0.35f), new Vector2(-0.9f,-0.5f), new Vector2(-0.35f,0.1f) }
            },
            new ShipDefinition {
                id = "wasp",
                priceType = PriceType.Coins, coinPrice = 2500, maxHealth = 90, speed = 7.5f, shotsPerCharge = 4,
                chargeTime = 8.5f, fireWindow = 3f, tier = 1, accent = new Color(1f, 0.85f, 0.2f),
                shape = new[] { new Vector2(0,1), new Vector2(0.2f,0.3f), new Vector2(0.95f,-0.2f), new Vector2(0.95f,-0.45f), new Vector2(0.2f,-0.3f), new Vector2(0.15f,-0.85f), new Vector2(-0.15f,-0.85f), new Vector2(-0.2f,-0.3f), new Vector2(-0.95f,-0.45f), new Vector2(-0.95f,-0.2f), new Vector2(-0.2f,0.3f) }
            },
            new ShipDefinition {
                id = "guardian",
                priceType = PriceType.Coins, coinPrice = 6000, maxHealth = 140, speed = 5.5f, shotsPerCharge = 4,
                chargeTime = 9f, fireWindow = 3.5f, tier = 2, accent = new Color(0.4f, 1f, 0.5f),
                shape = new[] { new Vector2(0,0.95f), new Vector2(0.5f,0.5f), new Vector2(0.75f,-0.2f), new Vector2(0.9f,-0.8f), new Vector2(0.3f,-0.6f), new Vector2(-0.3f,-0.6f), new Vector2(-0.9f,-0.8f), new Vector2(-0.75f,-0.2f), new Vector2(-0.5f,0.5f) }
            },
            new ShipDefinition {
                id = "phoenix",
                priceType = PriceType.RealMoney, productId = ProductIds.ShipPhoenix, maxHealth = 150, speed = 7f, shotsPerCharge = 6,
                chargeTime = 7f, fireWindow = 4f, tier = 3, accent = new Color(1f, 0.45f, 0.1f),
                shape = new[] { new Vector2(0,1), new Vector2(0.25f,0.4f), new Vector2(1f,0.1f), new Vector2(0.6f,-0.3f), new Vector2(0.9f,-0.9f), new Vector2(0.2f,-0.55f), new Vector2(0,-0.9f), new Vector2(-0.2f,-0.55f), new Vector2(-0.9f,-0.9f), new Vector2(-0.6f,-0.3f), new Vector2(-1f,0.1f), new Vector2(-0.25f,0.4f) }
            },
            new ShipDefinition {
                id = "titan",
                priceType = PriceType.RealMoney, productId = ProductIds.ShipTitan, maxHealth = 230, speed = 6f, shotsPerCharge = 7,
                chargeTime = 7f, fireWindow = 4f, tier = 3, accent = new Color(0.75f, 0.5f, 1f),
                shape = new[] { new Vector2(0,0.9f), new Vector2(0.45f,0.7f), new Vector2(0.6f,0.2f), new Vector2(1f,0f), new Vector2(1f,-0.6f), new Vector2(0.5f,-0.9f), new Vector2(-0.5f,-0.9f), new Vector2(-1f,-0.6f), new Vector2(-1f,0f), new Vector2(-0.6f,0.2f), new Vector2(-0.45f,0.7f) }
            },
            new ShipDefinition {
                id = "nova",
                priceType = PriceType.RealMoney, productId = ProductIds.ShipNova, maxHealth = 180, speed = 8.5f, shotsPerCharge = 8,
                chargeTime = 6.5f, fireWindow = 4.5f, tier = 3, accent = new Color(1f, 0.3f, 0.8f),
                shape = new[] { new Vector2(0,1), new Vector2(0.15f,0.2f), new Vector2(0.7f,0.35f), new Vector2(1f,-0.3f), new Vector2(0.3f,-0.25f), new Vector2(0.4f,-0.9f), new Vector2(0,-0.6f), new Vector2(-0.4f,-0.9f), new Vector2(-0.3f,-0.25f), new Vector2(-1f,-0.3f), new Vector2(-0.7f,0.35f), new Vector2(-0.15f,0.2f) }
            },
        };

        public static ShipDefinition Get(string id)
        {
            var s = All.Find(x => x.id == id);
            return s ?? All[0];
        }

        public static ShipDefinition ByProduct(string productId) => All.Find(x => x.productId == productId);
    }
}
