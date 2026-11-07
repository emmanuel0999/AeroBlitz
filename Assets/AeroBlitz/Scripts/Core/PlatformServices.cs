using System;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Funciones del sistema Android: pedir una valoración dentro del juego (In-App Review de Google Play)
    /// y abrir el menú de compartir del teléfono.
    /// </summary>
    public static class PlatformServices
    {
        public const string StoreUrl = "https://play.google.com/store/apps/details?id=com.frailescitystudios.aeroblitz";

        /// <summary>Niveles completados tras los que se pide valorar (como mucho una vez en cada uno).</summary>
        static readonly int[] ReviewLevels = { 5, 15, 30 };

        /// <summary>
        /// Pide la valoración si el jugador acaba de completar uno de los niveles elegidos por primera vez.
        /// Google decide si muestra la ventana (tiene un límite de veces por usuario), así que puede no aparecer.
        /// Las normas de Google prohíben dar recompensas a cambio de valorar: aquí no se da nada.
        /// </summary>
        public static void MaybeRequestReview(int completedLevel)
        {
            var d = GameManager.Instance.Data;
            if (Array.IndexOf(ReviewLevels, completedLevel) < 0 || completedLevel <= d.lastReviewPromptLevel) return;
            d.lastReviewPromptLevel = completedLevel;
            GameManager.Instance.Save();
            RequestReview();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Se guardan las referencias mientras dura la petición para que no las libere el recolector.
        static AndroidJavaObject _reviewManager, _activity;
        static TaskListener _listener;

        class TaskListener : AndroidJavaProxy
        {
            readonly Action<AndroidJavaObject> _done;
            public TaskListener(Action<AndroidJavaObject> done) : base("com.google.android.gms.tasks.OnCompleteListener") { _done = done; }
            public void onComplete(AndroidJavaObject task) => _done(task);
        }
#endif

        static void RequestReview()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    _activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using (var factory = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory"))
                    _reviewManager = factory.CallStatic<AndroidJavaObject>("create", _activity);

                _listener = new TaskListener(task =>
                {
                    try
                    {
                        if (!task.Call<bool>("isSuccessful")) { Debug.Log("[Review] Google no ofreció la valoración ahora."); return; }
                        using (var info = task.Call<AndroidJavaObject>("getResult"))
                            _reviewManager.Call<AndroidJavaObject>("launchReviewFlow", _activity, info);
                        Debug.Log("[Review] Ventana de valoración solicitada.");
                    }
                    catch (Exception e) { Debug.LogWarning("[Review] " + e.Message); }
                });
                using (var request = _reviewManager.Call<AndroidJavaObject>("requestReviewFlow"))
                    request.Call<AndroidJavaObject>("addOnCompleteListener", _listener);
            }
            catch (Exception e) { Debug.LogWarning("[Review] No se pudo pedir la valoración: " + e.Message); }
#else
            Debug.Log("[Review] (Editor) Aquí se pediría la valoración de Google Play.");
#endif
        }

        /// <summary>Abre el menú de compartir de Android con un texto (incluye el enlace de Google Play).</summary>
        public static void ShareText(string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                    intent.Call<AndroidJavaObject>("setType", "text/plain");
                    intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", text);
                    using (var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, L.T("share.chooser")))
                        activity.Call("startActivity", chooser);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Share] " + e.Message);
                GUIUtility.systemCopyBuffer = text;
                UIManager.Instance.Toast(L.T("share.copied"));
            }
#else
            GUIUtility.systemCopyBuffer = text;
            UIManager.Instance.Toast(L.T("share.copied"));
#endif
        }
    }
}
