using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Impide publicar un build de producción mientras los documentos legales tengan datos sin completar
    /// ("[COMPLETAR: ...]"). Los builds de desarrollo sí se permiten, para poder seguir probando.
    /// </summary>
    public class LegalBuildCheck : IPreprocessBuildWithReport
    {
        const string LegalFolder = "Assets/AeroBlitz/Resources/Legal";
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if ((report.summary.options & BuildOptions.Development) != 0) return;
            var pending = new System.Text.StringBuilder();
            foreach (var file in Directory.GetFiles(LegalFolder, "*.txt"))
                if (File.ReadAllText(file).Contains("[COMPLETAR"))
                    pending.AppendLine("  - " + Path.GetFileName(file));
            if (pending.Length == 0) return;
            throw new BuildFailedException(
                "[Aero Blitz] Build de producción cancelado: completa los datos marcados con [COMPLETAR: ...] en:\n" +
                pending + "(carpeta " + LegalFolder + ")");
        }

        [MenuItem("Aero Blitz/Revisar documentos legales")]
        static void CheckNow()
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(LegalFolder, "*.txt"))
                foreach (var line in File.ReadAllLines(file))
                    if (line.Contains("[COMPLETAR")) { count++; Debug.LogWarning($"{Path.GetFileName(file)}: {line.Trim()}"); }
            if (count == 0) Debug.Log("[Aero Blitz] Documentos legales completos.");
            else Debug.LogWarning($"[Aero Blitz] Faltan {count} datos por completar en los documentos legales.");
        }
    }
}
