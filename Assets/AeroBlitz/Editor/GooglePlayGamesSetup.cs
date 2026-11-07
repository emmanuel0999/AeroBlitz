using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Configura el plugin de Google Play Juegos con los datos de Play Console y Google Cloud,
    /// igual que el asistente "Window > Google Play Games > Setup > Android setup" pero sin ventanas.
    /// Ninguno de estos datos es secreto (van dentro del juego).
    /// </summary>
    public static class GooglePlayGamesSetup
    {
        // ID de la aplicación (= número del proyecto de Google Cloud "aero-blitz")
        const string AppId = "45662627700";
        // ID de cliente OAuth de tipo "Aplicación web" (Aero Blitz Servidor)
        const string WebClientId = "45662627700-6kg9vf4jbqfe87bagg4k90m42u3a9u84.apps.googleusercontent.com";

        [MenuItem("Aero Blitz/Configurar Google Play Juegos")]
        public static void Configure()
        {
            bool ok = GooglePlayGames.Editor.GPGSAndroidSetupUI.PerformSetup(WebClientId, AppId, null);
            Debug.Log("[Aero Blitz] Configuración de Google Play Juegos: " + (ok ? "OK" : "FALLÓ"));
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureBatch()
        {
            Configure();
            EditorApplication.Exit(0);
        }
    }
}
