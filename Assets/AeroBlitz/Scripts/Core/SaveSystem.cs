using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Guarda el progreso en JSON con una firma para detectar ediciones manuales
    /// (evita que alguien se regale monedas o vidas editando el archivo).
    /// </summary>
    public static class SaveSystem
    {
        const string FileName = "aeroblitz_save.json";
        const string Salt = "AeroBlitz::c9f2e1-cambia-esta-sal";

        [Serializable]
        class Envelope
        {
            public string data;
            public string sig;
        }

        static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                var env = new Envelope { data = json, sig = Sign(json) };
                string tmp = SavePath + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(env));
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tmp, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError("[Save] Error guardando: " + e);
            }
        }

        public static PlayerData Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                var env = JsonUtility.FromJson<Envelope>(File.ReadAllText(SavePath));
                if (env == null || string.IsNullOrEmpty(env.data)) return null;
                if (Sign(env.data) != env.sig)
                {
                    Debug.LogWarning("[Save] Firma inválida: archivo modificado. Se reinicia el progreso.");
                    return null;
                }
                return JsonUtility.FromJson<PlayerData>(env.data);
            }
            catch (Exception e)
            {
                Debug.LogError("[Save] Error cargando: " + e);
                return null;
            }
        }

        public static void DeleteAll()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        static string Sign(string json)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Salt + json));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
