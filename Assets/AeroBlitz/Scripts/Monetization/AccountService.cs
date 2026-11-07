using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if AEROBLITZ_UGS
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif
#if AEROBLITZ_CLOUDSAVE
using Unity.Services.CloudSave;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace AeroBlitz
{
    /// <summary>
    /// Cuenta opcional con Google Play Juegos + respaldo de TODO el progreso en Unity Cloud Save.
    /// - Sin iniciar sesión el juego funciona igual (solo guardado local).
    /// - Al iniciar sesión, el progreso se sube a la nube tras cada cambio.
    /// - Al reinstalar o cambiar de teléfono, iniciar sesión con la misma cuenta de Google recupera todo.
    /// </summary>
    public class AccountService : MonoBehaviour
    {
        public static AccountService Instance { get; private set; }

        public bool Busy { get; private set; }
        public bool IsLinked => GameManager.Instance.Data.googleLinked;

        float _uploadTimer = -1f;
        bool _uploading, _uploadAgain;

        static bool SimulationAllowed => Application.isEditor || Debug.isDebugBuild;

        void Awake() { Instance = this; }

        /// <summary>Unity Gaming Services listo y con sesión iniciada.</summary>
        public static bool CloudAvailable
        {
            get
            {
#if AEROBLITZ_UGS && AEROBLITZ_CLOUDSAVE
                return UnityServices.State == ServicesInitializationState.Initialized
                       && AuthenticationService.Instance.IsSignedIn;
#else
                return false;
#endif
            }
        }

        // ---------------- SUBIDA AUTOMÁTICA ----------------
        /// <summary>Se llama en cada GameManager.Save(): sube el progreso unos segundos después.</summary>
        public void ScheduleUpload()
        {
            if (IsLinked) _uploadTimer = GameConfig.CloudUploadDelay;
        }

        void Update()
        {
            if (_uploadTimer < 0f) return;
            _uploadTimer -= Time.unscaledDeltaTime;
            if (_uploadTimer <= 0f) { _uploadTimer = -1f; _ = UploadAsync(); }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused && IsLinked) { _uploadTimer = -1f; _ = UploadAsync(); }
            // Al volver de segundo plano la sesión puede haber caducado: se renueva antes de usarla
            if (!paused) _ = RefreshSessionIfExpiredAsync();
        }

        /// <summary>Renueva la sesión de Unity (mismo jugador, con el token guardado) si ha caducado.</summary>
        async Task RefreshSessionIfExpiredAsync(bool force = false)
        {
#if AEROBLITZ_UGS
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized) return;
                var auth = AuthenticationService.Instance;
                if (!force && auth.IsSignedIn && !auth.IsExpired) return;
                if (auth.IsSignedIn) auth.SignOut(false); // conserva el token de sesión para volver como el mismo jugador
                await auth.SignInAnonymouslyAsync();
                Debug.Log("[Cuenta] Sesión renovada.");
            }
            catch (Exception e) { Debug.LogWarning("[Cuenta] No se pudo renovar la sesión: " + e.Message); }
#else
            await Task.CompletedTask;
#endif
        }

        static bool IsSessionError(Exception e)
        {
            string m = (e.Message ?? "").ToLowerInvariant();
            return m.Contains("expired") || m.Contains("jwt") || m.Contains("unauthorized") || m.Contains("401");
        }

        public async Task<bool> UploadAsync()
        {
            if (!IsLinked || !CloudAvailable) return false;
            if (_uploading) { _uploadAgain = true; return false; }
            _uploading = true;
            try
            {
#if AEROBLITZ_CLOUDSAVE
                try { await SaveToCloudAsync(); }
                catch (Exception e) when (IsSessionError(e))
                {
                    // La sesión caducó: se renueva y se reintenta una vez
                    Debug.Log("[Cuenta] Sesión caducada al subir el progreso; se renueva y se reintenta.");
                    await RefreshSessionIfExpiredAsync(true);
                    await SaveToCloudAsync();
                }
#endif
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Cuenta] No se pudo subir el progreso: " + e.Message);
                _uploadTimer = 30f; // se vuelve a intentar más tarde para no perder el respaldo
                return false;
            }
            finally
            {
                _uploading = false;
                if (_uploadAgain) { _uploadAgain = false; _uploadTimer = 1f; }
            }
        }

#if AEROBLITZ_CLOUDSAVE
        static Task SaveToCloudAsync()
        {
            string json = JsonUtility.ToJson(GameManager.Instance.Data);
            return CloudSaveService.Instance.Data.Player.SaveAsync(
                new Dictionary<string, object> { { GameConfig.CloudSaveKey, json } },
                new Unity.Services.CloudSave.Models.Data.Player.SaveOptions());
        }
#endif

        async Task<PlayerData> DownloadAsync()
        {
#if AEROBLITZ_CLOUDSAVE
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { GameConfig.CloudSaveKey },
                new Unity.Services.CloudSave.Models.Data.Player.LoadOptions());
            if (result.TryGetValue(GameConfig.CloudSaveKey, out var item))
            {
                var data = JsonUtility.FromJson<PlayerData>(item.Value.GetAs<string>());
                if (data != null && data.ships.Count > 0) return data;
            }
#else
            await Task.Yield();
#endif
            return null;
        }

        // ---------------- INICIAR SESIÓN CON GOOGLE ----------------
        /// <summary>
        /// Inicia sesión con Google Play Juegos. Si esa cuenta ya tenía progreso guardado
        /// (reinstalación u otro teléfono), lo recupera. done(ok, mensaje).
        /// </summary>
        public void SignInWithGoogle(Action<bool, string> done)
        {
            if (Busy) return;
            if (!CloudAvailable)
            {
                // En el Editor sin proyecto de Unity Cloud vinculado se simula para poder probar la interfaz.
                if (SimulationAllowed) { SimulateLink(done); return; }
                done?.Invoke(false, L.T("acc.offline"));
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            Busy = true;
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success) { RequestCodeThenLink(done); return; }
                PlayGamesPlatform.Instance.ManuallyAuthenticate(status2 =>
                {
                    if (status2 == SignInStatus.Success) RequestCodeThenLink(done);
                    else Finish(done, false, L.T("acc.google_cancelled"));
                });
            });
#else
            if (SimulationAllowed) { SimulateLink(done); return; }
            done?.Invoke(false, L.T("acc.unsupported"));
#endif
        }

        void SimulateLink(Action<bool, string> done)
        {
            Debug.Log("[Cuenta] Inicio de sesión con Google SIMULADO (Editor / build de desarrollo).");
            MarkLinked();
            done?.Invoke(true, L.T("acc.simulated"));
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void RequestCodeThenLink(Action<bool, string> done)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
            {
                if (string.IsNullOrEmpty(code)) { Finish(done, false, L.T("acc.google_error")); return; }
                _ = LinkOrRecoverAsync(code, done);
            });
        }

        async Task LinkOrRecoverAsync(string code, Action<bool, string> done)
        {
            try
            {
                // Caso normal: esta cuenta de Google es nueva → se vincula al jugador actual.
                await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code);
                MarkLinked();
                await UploadAsync();
                Finish(done, true, L.T("acc.linked"));
            }
            catch (AuthenticationException e) when (e.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // Esta cuenta de Google ya tiene progreso (otro teléfono o reinstalación): se recupera.
                // El código de Google es de un solo uso, así que se pide uno nuevo.
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, code2 =>
                {
                    if (string.IsNullOrEmpty(code2)) { Finish(done, false, L.T("acc.google_error")); return; }
                    _ = RecoverExistingAsync(code2, done);
                });
            }
            catch (Exception e)
            {
                Finish(done, false, ErrorMessage(e));
            }
        }

        async Task RecoverExistingAsync(string code, Action<bool, string> done)
        {
            AuthenticationService.Instance.SignOut(); // conserva la sesión anterior por si algo falla
            try
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
            }
            catch (Exception e)
            {
                try { await AuthenticationService.Instance.SignInAnonymouslyAsync(); } catch { }
                Finish(done, false, ErrorMessage(e));
                return;
            }

            PlayerData cloud = null;
            try { cloud = await DownloadAsync(); }
            catch (Exception e) { Debug.LogWarning("[Cuenta] No se pudo leer la nube: " + e.Message); }

            var local = GameManager.Instance.Data;
            if (cloud == null)
            {
                MarkLinked();
                await UploadAsync();
                Finish(done, true, L.T("acc.linked"));
            }
            else if (IsFresh(local))
            {
                ApplyCloud(cloud);
                Finish(done, true, L.T("acc.restored"));
            }
            else
            {
                Busy = false;
                // Los dos tienen progreso: el jugador elige cuál conservar.
                UIManager.Instance.ShowProgressConflict(cloud, local,
                    useCloud: () => { ApplyCloud(cloud); done?.Invoke(true, L.T("acc.restored")); },
                    useLocal: async () => { MarkLinked(); await UploadAsync(); done?.Invoke(true, L.T("acc.linked")); });
            }
        }
#endif

        /// <summary>Progreso "vacío": instalación nueva sin avances reales.</summary>
        static bool IsFresh(PlayerData d) =>
            !d.registered || (d.currentLevel <= 1 && d.totalPoints == 0 && d.ownedProducts.Count == 0 && d.ships.Count <= 1);

        void MarkLinked()
        {
            var gm = GameManager.Instance;
            gm.Data.googleLinked = true;
            gm.Save();
            gm.NotifyChanged();
        }

        void ApplyCloud(PlayerData cloud)
        {
            var local = GameManager.Instance.Data;
            cloud.googleLinked = true;
            cloud.language = local.language;           // el idioma es del teléfono, no de la cuenta
            if (cloud.acceptedTermsVersion < local.acceptedTermsVersion)
            {
                cloud.acceptedTermsVersion = local.acceptedTermsVersion;
                cloud.acceptedTermsUtc = local.acceptedTermsUtc;
            }
            GameManager.Instance.ReplaceData(cloud);
            if (cloud.registered)
                _ = LeaderboardService.Instance.SyncProfileAsync(cloud.username, cloud.countryCode, cloud.totalPoints);
        }

        void Finish(Action<bool, string> done, bool ok, string msg)
        {
            Busy = false;
            done?.Invoke(ok, msg);
        }

        // ---------------- ELIMINAR CUENTA (exigido por Google Play) ----------------
        /// <summary>Borra el progreso en la nube, la cuenta de Unity Gaming Services y el progreso local.</summary>
        public async void DeleteAccountAndData(Action<bool, string> done)
        {
            if (Busy) return;
            Busy = true;
            try
            {
#if AEROBLITZ_UGS && AEROBLITZ_CLOUDSAVE
                if (CloudAvailable)
                {
                    try
                    {
                        await CloudSaveService.Instance.Data.Player.DeleteAsync(GameConfig.CloudSaveKey,
                            new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions());
                    }
                    catch (Exception e) { Debug.Log("[Cuenta] Sin datos en la nube para borrar: " + e.Message); }
                    // El puntaje no se puede borrar desde el juego: se anonimiza el nombre y el ranking oculta esas entradas.
                    try
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync(GameConfig.DeletedPlayerName);
                        await LeaderboardService.Instance.RefreshEntryAsync();
                    }
                    catch (Exception e) { Debug.LogWarning("[Cuenta] No se pudo anonimizar el ranking: " + e.Message); }
                    await AuthenticationService.Instance.DeleteAccountAsync();
                    try { await AuthenticationService.Instance.SignInAnonymouslyAsync(); } catch { }
                }
#else
                await Task.Yield();
#endif
                Busy = false;
                GameManager.Instance.ResetProgress();
                done?.Invoke(true, L.T("acc.deleted"));
            }
            catch (Exception e)
            {
                Busy = false;
                done?.Invoke(false, ErrorMessage(e));
            }
        }

        static string ErrorMessage(Exception e)
        {
            Debug.LogWarning("[Cuenta] " + e);
#if AEROBLITZ_UGS
            if (e is RequestFailedException rf && rf.ErrorCode == CommonErrorCodes.TransportError) return L.T("acc.offline");
#endif
            return L.T("acc.error");
        }
    }
}
