using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>Importa las imágenes de Resources/UI (logo, fondo, iconos del menú) como sprites de interfaz.</summary>
    public class UITextureImport : AssetPostprocessor
    {
        const string Folder = "Assets/AeroBlitz/Resources/UI";

        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').StartsWith(Folder + "/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = 2048;
            // El fondo (JPG) se comprime; logo e iconos sin compresión para que se vean nítidos
            ti.textureCompression = assetPath.EndsWith(".jpg") ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
        }

        [MenuItem("Aero Blitz/Reimportar imágenes del menú")]
        public static void ReimportUI()
        {
            AssetDatabase.ImportAsset(Folder, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            Debug.Log("[Aero Blitz] Imágenes del menú reimportadas.");
        }
    }
}
