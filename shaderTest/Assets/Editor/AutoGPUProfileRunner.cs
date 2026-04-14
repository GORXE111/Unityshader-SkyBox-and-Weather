using UnityEngine;
using UnityEditor;
using System.IO;

namespace GTA5Sky.Tests
{
    [InitializeOnLoad]
    public static class AutoGPUProfileRunner
    {
        static readonly string TriggerFile = "E:/ShaderUnityTest/run-gpu-profile.trigger";
        static readonly string ReadyFlag = "E:/ShaderUnityTest/gpu-profile-ready.flag";

        static AutoGPUProfileRunner()
        {
            if (!File.Exists(TriggerFile)) return;
            File.Delete(TriggerFile);

            EditorApplication.delayCall += () =>
            {
                // Enable frame timing stats in player settings
                PlayerSettings.enableFrameTimingStats = true;

                Debug.Log("[AutoGPUProfile] Trigger detected. Enabling frame timing + entering Play...");
                File.WriteAllText(ReadyFlag, "auto");
                EditorApplication.isPlaying = true;
            };
        }
    }
}
