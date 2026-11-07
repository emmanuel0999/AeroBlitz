using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if AEROBLITZ_UGS
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
#endif

namespace AeroBlitz
{
    public class RankingEntry
    {
        public int rank;
        public string name;
        public string country;
        public long points;
        public bool isMe;
    }

    /// <summary>
    /// Ranking mundial y por país usando Unity Gaming Services Leaderboards.
    /// El país se guarda como metadata de cada puntaje. Sin conexión / sin proyecto enlazado
    /// muestra solo al jugador local.
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        public static LeaderboardService Instance { get; private set; }
        public bool Online { get; private set; }

        [Serializable] class Meta { public string country; }

        void Awake() { Instance = this; }

        public async Task InitializeAsync()
        {
#if AEROBLITZ_UGS
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Online = true;
                Debug.Log("[Ranking] Conectado a UGS. PlayerId: " + AuthenticationService.Instance.PlayerId);
                var d = GameManager.Instance.Data;
                if (d.registered) await SyncProfileAsync(d.username, d.countryCode, d.totalPoints);
            }
            catch (Exception e)
            {
                Online = false;
                Debug.LogWarning("[Ranking] Modo sin conexión (enlaza el proyecto a Unity Cloud para activarlo): " + e.Message);
            }
#else
            await Task.CompletedTask;
#endif
        }

        static string SanitizeName(string n)
        {
            // UGS no permite espacios en el nombre del jugador
            n = (n ?? "Piloto").Trim().Replace(' ', '_');
            return n.Length > 20 ? n.Substring(0, 20) : n;
        }

        public async Task SyncProfileAsync(string username, string country, long points)
        {
#if AEROBLITZ_UGS
            if (!Online) return;
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(SanitizeName(username));
                await SubmitAsync(points, country);
            }
            catch (Exception e) { Debug.LogWarning("[Ranking] No se pudo sincronizar perfil: " + e.Message); }
#else
            await Task.CompletedTask;
#endif
        }

        public async void SubmitScore(long points, string country)
        {
            try { await SubmitAsync(points, country); }
            catch (Exception e) { Debug.LogWarning("[Ranking] Error enviando puntaje: " + e.Message); }
        }

        /// <summary>Vuelve a enviar el puntaje actual para que la entrada tome el nombre de jugador vigente.</summary>
        public async Task RefreshEntryAsync()
        {
            var d = GameManager.Instance.Data;
            await SubmitAsync(d.totalPoints, d.countryCode ?? "OT");
        }

        async Task SubmitAsync(long points, string country)
        {
#if AEROBLITZ_UGS
            if (!Online) return;
            var options = new AddPlayerScoreOptions { Metadata = new Dictionary<string, string> { { "country", country } } };
            await LeaderboardsService.Instance.AddPlayerScoreAsync(GameConfig.LeaderboardId, points, options);
#else
            await Task.CompletedTask;
#endif
        }

        /// <summary>countryFilter = null para ranking mundial.</summary>
        public async void Fetch(string countryFilter, Action<List<RankingEntry>, bool> onDone)
        {
            var list = new List<RankingEntry>();
            bool online = false;
#if AEROBLITZ_UGS
            if (Online)
            {
                try
                {
                    string myId = AuthenticationService.Instance.PlayerId;
                    var page = await LeaderboardsService.Instance.GetScoresAsync(GameConfig.LeaderboardId,
                        new GetScoresOptions { Offset = 0, Limit = GameConfig.LeaderboardFetchLimit, IncludeMetadata = true });
                    foreach (var r in page.Results)
                    {
                        string country = "OT";
                        if (!string.IsNullOrEmpty(r.Metadata))
                        {
                            try { country = JsonUtility.FromJson<Meta>(r.Metadata).country ?? "OT"; } catch { }
                        }
                        if (countryFilter != null && country != countryFilter) continue;
                        string name = r.PlayerName ?? "Piloto";
                        int hash = name.LastIndexOf('#');
                        if (hash > 0) name = name.Substring(0, hash);
                        if (name == GameConfig.DeletedPlayerName) continue; // cuenta eliminada: no se muestra
                        list.Add(new RankingEntry
                        {
                            name = name.Replace('_', ' '),
                            country = country,
                            points = (long)r.Score,
                            isMe = r.PlayerId == myId
                        });
                    }
                    online = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Ranking] Error obteniendo ranking: " + e.Message);
                }
            }
#else
            await Task.Yield();
#endif
            if (!online)
            {
                var d = GameManager.Instance.Data;
                if (countryFilter == null || countryFilter == d.countryCode)
                    list.Add(new RankingEntry { name = d.username, country = d.countryCode, points = d.totalPoints, isMe = true });
            }
            list.Sort((a, b) => b.points.CompareTo(a.points));
            for (int i = 0; i < list.Count; i++) list[i].rank = i + 1;
            onDone?.Invoke(list, online);
        }
    }
}
