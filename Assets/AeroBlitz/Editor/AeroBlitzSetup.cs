using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>Menú "Aero Blitz > Configurar proyecto": crea la escena y aplica ajustes de publicación.</summary>
    public static class AeroBlitzSetup
    {
        const string ScenePath = "Assets/AeroBlitz/Scenes/Main.unity";
        // Identificador único del juego en Google Play / App Store. NO cambiar tras publicar.
        const string BundleId = "com.frailescitystudios.aeroblitz";
        const string CompanyName = "Frailes City Studios";

        [MenuItem("Aero Blitz/Configurar proyecto")]
        public static void Setup()
        {
            PlayerSettings.productName = "Aero Blitz";
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.bundleVersion = "1.1.0"; // versión visible en la tienda; súbela en cada actualización (1.0.2 = misiones; 1.0.3 = naves nuevas, menú nuevo, 5 vidas; 1.1.0 = jefes, Ruta Galáctica, tutorial)
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, BundleId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            // Cada archivo que subas a Google Play necesita un número de versión MAYOR que el anterior.
            // Última generada: 16 (AeroBlitz-v16.aab, 1.0.3: lo de la 15 + iconos en todas las pantallas y gráficos más nítidos). La próxima es la 17. Sube este mínimo en cada nueva versión.
            const int MinVersionCode = 18;
            if (PlayerSettings.Android.bundleVersionCode < MinVersionCode)
                PlayerSettings.Android.bundleVersionCode = MinVersionCode;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            // Google Play Juegos v2 (22.x) exige Android 7.0 (API 24) como mínimo.
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            if (!System.IO.File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cam = new GameObject("Main Camera") { tag = "MainCamera" };
                var c = cam.AddComponent<Camera>();
                c.orthographic = true;
                c.orthographicSize = 8;
                c.clearFlags = CameraClearFlags.SolidColor;
                c.backgroundColor = new Color(0.02f, 0.03f, 0.09f);
                cam.transform.position = new Vector3(0, 0, -10);
                cam.AddComponent<AudioListener>();
                new GameObject("AeroBlitz").AddComponent<GameManager>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[Aero Blitz] Proyecto configurado. Abre Assets/AeroBlitz/Scenes/Main.unity y presiona Play.");
        }

        /// <summary>Para ejecutar desde línea de comandos (-executeMethod).</summary>
        public static void SetupBatch()
        {
            Setup();
            EditorApplication.Exit(0);
        }

        /// <summary>Build WebGL de desarrollo para probar rápido en el navegador.</summary>
        public static void BuildWebGLTest()
        {
            Setup();
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.Development
            });
            Debug.Log("[Aero Blitz] Build WebGL: " + report.summary.result);
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>
        /// APK de prueba (desarrollo) firmado con la clave de depuración de Unity: sirve para comprobar
        /// que el proyecto compila completo para Android sin necesitar las contraseñas del keystore.
        /// NO es la versión para Google Play (esa es un AAB firmado con tu clave).
        /// </summary>
        public static void BuildAndroidTestApkBatch()
        {
            Setup();
            bool hadCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            bool wasAppBundle = EditorUserBuildSettings.buildAppBundle;
            UnityEditor.Build.Reporting.BuildResult result;
            try
            {
                PlayerSettings.Android.useCustomKeystore = false;
                EditorUserBuildSettings.buildAppBundle = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = "Builds/Android/AeroBlitz-test.apk",
                    target = BuildTarget.Android,
                    options = BuildOptions.Development
                });
                result = report.summary.result;
                Debug.Log($"[Aero Blitz] Build Android de prueba: {result} ({report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores)");
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore = hadCustomKeystore;
                EditorUserBuildSettings.buildAppBundle = wasAppBundle;
                AssetDatabase.SaveAssets();
            }
            EditorApplication.Exit(result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Compila los scripts tal como en un build de Android (incluye código solo-dispositivo).</summary>
        public static void CompileAndroidPlayerScriptsBatch()
        {
            var settings = new UnityEditor.Build.Player.ScriptCompilationSettings
            {
                group = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                options = UnityEditor.Build.Player.ScriptCompilationOptions.None
            };
            var result = UnityEditor.Build.Player.PlayerBuildInterface.CompilePlayerScripts(settings, "Temp/AndroidPlayerScripts");
            bool ok = result.assemblies != null && result.assemblies.Contains("AeroBlitz.dll");
            Debug.Log("[Aero Blitz] Compilación de scripts de Android: " + (ok ? "OK" : "ERROR"));
            EditorApplication.Exit(ok ? 0 : 1);
        }

        [MenuItem("Aero Blitz/Borrar partida guardada")]
        public static void DeleteSave()
        {
            SaveSystem.DeleteAll();
            Debug.Log("[Aero Blitz] Partida guardada eliminada.");
        }
    }
}
