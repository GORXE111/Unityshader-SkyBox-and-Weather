using UnityEngine;
using UnityEditor;
using System.IO;

namespace GTA5Sky.Tests
{
    [InitializeOnLoad]
    public static class AutoProfilerExportRunner
    {
        static readonly string TriggerFile = "E:/ShaderUnityTest/run-profiler-export.trigger";
        static readonly string ReadyFlag = "E:/ShaderUnityTest/profiler-export-ready.flag";

        static AutoProfilerExportRunner()
        {
            if (!File.Exists(TriggerFile)) return;
            File.Delete(TriggerFile);

            EditorApplication.delayCall += () =>
            {
                Debug.Log("[AutoProfilerExport] Trigger detected. Entering Play...");
                File.WriteAllText(ReadyFlag, "auto");
                EditorApplication.isPlaying = true;
            };
        }
    }
}
