using UnityEngine;
using UnityEditor;
using System.IO;

namespace GTA5Sky.Tests
{
    /// <summary>
    /// File-triggered benchmark runner.
    /// Create E:/ShaderUnityTest/run-benchmark.trigger → Unity enters Play → benchmark runs → exits Play → results saved.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoBenchmarkRunner
    {
        static readonly string TriggerFile = "E:/ShaderUnityTest/run-benchmark.trigger";
        static readonly string ReadyFlag = "E:/ShaderUnityTest/benchmark-ready.flag";

        static AutoBenchmarkRunner()
        {
            if (!File.Exists(TriggerFile)) return;
            File.Delete(TriggerFile);

            EditorApplication.delayCall += () =>
            {
                Debug.Log("[AutoBenchmark] Trigger detected. Entering Play mode...");
                // Write a flag so the PlayMode component knows to auto-start
                File.WriteAllText(ReadyFlag, "auto");
                EditorApplication.isPlaying = true;
            };
        }
    }
}
