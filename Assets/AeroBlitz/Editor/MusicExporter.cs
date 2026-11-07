using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>Exporta los temas musicales (uno por mundo) a WAV (carpeta MusicPreview) para poder escucharlos en el PC.</summary>
    public static class MusicExporter
    {
        const string Folder = "MusicPreview";

        [MenuItem("Aero Blitz/Exportar música a WAV")]
        public static void Export()
        {
            Directory.CreateDirectory(Folder);
            for (int i = 0; i < MusicPlayer.TrackCount; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                float[] s = MusicPlayer.RenderTrack(i);
                watch.Stop();

                double sum = 0; float peak = 0; int bad = 0;
                foreach (float v in s)
                {
                    if (float.IsNaN(v) || float.IsInfinity(v)) { bad++; continue; }
                    sum += v * v;
                    if (Math.Abs(v) > peak) peak = Math.Abs(v);
                }
                double rms = Math.Sqrt(sum / s.Length);
                int from = i * WorldCatalog.LevelsPerWorld + 1, to = from + WorldCatalog.LevelsPerWorld - 1;
                string path = $"{Folder}/tema_{i + 1:00}_niveles_{from}-{to}.wav";
                WriteWav(path, s, MusicPlayer.SampleRate);
                Debug.Log($"[Aero Blitz] Tema {i + 1}: {s.Length / (float)MusicPlayer.SampleRate:0.0}s  pico={peak:0.00}  rms={rms:0.000}  invalidos={bad}  generado en {watch.ElapsedMilliseconds} ms -> {path}");
            }
        }

        public static void ExportBatch()
        {
            Export();
            EditorApplication.Exit(0);
        }

        static void WriteWav(string path, float[] samples, int rate)
        {
            using (var w = new BinaryWriter(File.Create(path)))
            {
                int bytes = samples.Length * 2;
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + bytes);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); w.Write(16);
                w.Write((short)1); w.Write((short)1); w.Write(rate); w.Write(rate * 2); w.Write((short)2); w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(bytes);
                foreach (float v in samples) w.Write((short)(Mathf.Clamp(v, -1f, 1f) * 32767f));
            }
        }
    }
}
