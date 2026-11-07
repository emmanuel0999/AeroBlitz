using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Exporta los iconos de los productos de compra en PNG de 512x512 (carpeta StoreAssets/iconos_productos)
    /// para subirlos en Play Console, y una hoja con todos los iconos del juego para revisarlos.
    /// </summary>
    public static class ProductIconExporter
    {
        const string Folder = "StoreAssets/iconos_productos";

        [MenuItem("Aero Blitz/Exportar iconos de productos")]
        public static void Export()
        {
            Directory.CreateDirectory(Folder);
            foreach (var p in ProductCatalog.All)
            {
                var tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                tex.SetPixels(IconPainter.Paint(IconPainter.KeyForProduct(p.id), 512));
                tex.Apply();
                File.WriteAllBytes($"{Folder}/{p.id}.png", tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }
            Debug.Log($"[Aero Blitz] {ProductCatalog.All.Count} iconos de productos exportados a {Folder}");

            // Hoja de revisión con todos los iconos que usa el juego
            var keys = new List<string>();
            foreach (var p in ProductCatalog.All) keys.Add(IconPainter.KeyForProduct(p.id));
            keys.AddRange(new[] { "free_life", "free_coins", "free_upgrade", "daily", "eq:eq_plates", "eq:eq_magazine",
                "up:Armor", "up:Engine", "up:Magazine", "up:Charger" });
            foreach (var s in ShipCatalog.All) if (s.priceType == PriceType.Coins) keys.Add("ship:" + s.id);
            foreach (var pt in UpgradeCatalog.Paints) if (pt.priceType == PriceType.Coins) keys.Add("paint:" + pt.id);

            const int cell = 128, cols = 8;
            int rows = (keys.Count + cols - 1) / cols;
            var sheet = new Texture2D(cols * cell, rows * cell, TextureFormat.RGBA32, false);
            var clear = new Color[cols * cell * rows * cell];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0.08f, 0.09f, 0.14f, 1f);
            sheet.SetPixels(clear);
            for (int i = 0; i < keys.Count; i++)
            {
                var px = IconPainter.Paint(keys[i], cell);
                int ox = (i % cols) * cell, oy = (rows - 1 - i / cols) * cell;
                for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                    {
                        var c = px[y * cell + x];
                        var under = sheet.GetPixel(ox + x, oy + y);
                        sheet.SetPixel(ox + x, oy + y, Color.Lerp(under, c, c.a));
                    }
            }
            sheet.Apply();
            File.WriteAllBytes("StoreAssets/iconos_todos.png", sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
            Debug.Log("[Aero Blitz] Hoja de iconos: StoreAssets/iconos_todos.png (" + string.Join(", ", keys) + ")");
        }

        public static void ExportBatch()
        {
            Export();
            EditorApplication.Exit(0);
        }
    }
}
