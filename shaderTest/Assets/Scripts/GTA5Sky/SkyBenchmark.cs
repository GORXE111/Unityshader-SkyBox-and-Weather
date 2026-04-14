using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace GTA5Sky
{
    /// <summary>
    /// Automated sky benchmark: runs a full day/night cycle, captures screenshots
    /// at key moments, and logs frame timing statistics.
    /// Press F5 to start benchmark. Results saved to Application.persistentDataPath/SkyBenchmark/
    /// </summary>
    public sealed class SkyBenchmark : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<SkyBenchmark>() != null) return;
            GameObject go = new GameObject("SkyBenchmark");
            go.AddComponent<SkyBenchmark>();
            DontDestroyOnLoad(go);
        }

        static readonly string ReadyFlag = "E:/ShaderUnityTest/benchmark-ready.flag";
        bool isRunning;
        bool autoMode;
        string outputDir;

        // Timing data
        readonly System.Collections.Generic.List<FrameSample> samples = new();

        struct FrameSample
        {
            public float timeOfDay;
            public float frameMs;
            public float cpuBuildMs;
        }

        // Key screenshot times: dawn, morning, noon, afternoon, dusk, night
        static readonly float[] screenshotTimes = { 5.5f, 7f, 9f, 12f, 15f, 18f, 20f, 22f, 0.5f, 3f };

        void Start()
        {
            // Auto-start if triggered by AutoBenchmarkRunner
            if (File.Exists(ReadyFlag))
            {
                File.Delete(ReadyFlag);
                autoMode = true;
                StartCoroutine(RunBenchmark());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5) && !isRunning)
            {
                autoMode = false;
                StartCoroutine(RunBenchmark());
            }
        }

        IEnumerator RunBenchmark()
        {
            isRunning = true;
            samples.Clear();

            outputDir = Path.Combine(Application.persistentDataPath, "SkyBenchmark",
                System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(outputDir);

            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
            if (cycle == null)
            {
                Debug.LogError("[SkyBenchmark] No DayNightCycle found");
                isRunning = false;
                yield break;
            }

            float originalSpeed = cycle.DaySpeed;
            float originalTime = cycle.TimeOfDay;

            // Run through 24 hours at accelerated speed
            float benchmarkSpeed = 4f; // 6 seconds per game-hour = 144s total
            cycle.DaySpeed = benchmarkSpeed;
            cycle.SetTimeOfDay(0f);

            Debug.Log($"[SkyBenchmark] Started. Output: {outputDir}");

            int screenshotIdx = 0;
            float[] sortedTimes = (float[])screenshotTimes.Clone();
            System.Array.Sort(sortedTimes);

            var sw = new System.Diagnostics.Stopwatch();
            float startRealTime = Time.realtimeSinceStartup;

            while (cycle.TimeOfDay < 23.95f || Time.realtimeSinceStartup - startRealTime < 2f)
            {
                float tod = cycle.TimeOfDay;

                // Measure Build cost
                sw.Restart();
                if (WeatherController.Instance != null)
                {
                    var state = WeatherController.Instance.GetCurrentWeatherState();
                    GTA5TimecycleSky.Build(state, tod, Time.timeSinceLevelLoad);
                }
                sw.Stop();

                samples.Add(new FrameSample
                {
                    timeOfDay = tod,
                    frameMs = Time.unscaledDeltaTime * 1000f,
                    cpuBuildMs = (float)sw.Elapsed.TotalMilliseconds
                });

                // Screenshot at key times
                if (screenshotIdx < sortedTimes.Length)
                {
                    float target = sortedTimes[screenshotIdx];
                    if (tod >= target - 0.05f && tod <= target + 0.3f)
                    {
                        yield return new WaitForEndOfFrame();
                        CaptureScreenshot(tod);
                        screenshotIdx++;
                    }
                }

                // Stop after one full cycle
                if (tod > 23.9f)
                    break;

                yield return null;
            }

            // Restore
            cycle.DaySpeed = originalSpeed;
            cycle.SetTimeOfDay(originalTime);

            // Generate report
            GenerateReport();

            Debug.Log($"[SkyBenchmark] Complete. {samples.Count} frames, {screenshotIdx} screenshots. See: {outputDir}");
            isRunning = false;

            #if UNITY_EDITOR
            if (autoMode)
            {
                Debug.Log("[SkyBenchmark] Auto mode — exiting Play mode.");
                UnityEditor.EditorApplication.isPlaying = false;
            }
            #endif
        }

        void CaptureScreenshot(float timeOfDay)
        {
            int w = Screen.width;
            int h = Screen.height;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            string filename = $"sky_{timeOfDay:F1}h.png";
            File.WriteAllBytes(Path.Combine(outputDir, filename), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        void GenerateReport()
        {
            if (samples.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Sky Benchmark Report");
            sb.AppendLine($"Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Frames sampled: {samples.Count}");
            sb.AppendLine($"Resolution: {Screen.width}x{Screen.height}");
            sb.AppendLine();

            // Compute stats
            float totalFrameMs = 0, totalBuildMs = 0;
            float maxFrameMs = 0, maxBuildMs = 0;
            float minFrameMs = float.MaxValue;
            float maxFrameTod = 0, maxBuildTod = 0;

            // Per-phase stats
            float[] phaseFrameSum = new float[4]; // dawn, day, dusk, night
            int[] phaseCount = new int[4];
            float[] phaseBuildSum = new float[4];

            foreach (var s in samples)
            {
                totalFrameMs += s.frameMs;
                totalBuildMs += s.cpuBuildMs;
                if (s.frameMs > maxFrameMs) { maxFrameMs = s.frameMs; maxFrameTod = s.timeOfDay; }
                if (s.frameMs < minFrameMs) minFrameMs = s.frameMs;
                if (s.cpuBuildMs > maxBuildMs) { maxBuildMs = s.cpuBuildMs; maxBuildTod = s.timeOfDay; }

                int phase = GetPhase(s.timeOfDay);
                phaseFrameSum[phase] += s.frameMs;
                phaseBuildSum[phase] += s.cpuBuildMs;
                phaseCount[phase]++;
            }

            float avgFrameMs = totalFrameMs / samples.Count;
            float avgBuildMs = totalBuildMs / samples.Count;
            float avgFps = 1000f / avgFrameMs;

            sb.AppendLine("## Overall");
            sb.AppendLine($"- Avg FPS: {avgFps:F1}");
            sb.AppendLine($"- Avg frame: {avgFrameMs:F2}ms (min {minFrameMs:F2}, max {maxFrameMs:F2} at {maxFrameTod:F1}h)");
            sb.AppendLine($"- Avg Build CPU: {avgBuildMs:F4}ms (max {maxBuildMs:F4} at {maxBuildTod:F1}h)");
            sb.AppendLine($"- Build % of frame: {(avgBuildMs / avgFrameMs * 100):F3}%");
            sb.AppendLine();

            string[] phaseNames = { "Dawn (5-7h)", "Day (7-18h)", "Dusk (18-21h)", "Night (21-5h)" };
            sb.AppendLine("## Per Phase");
            sb.AppendLine("| Phase | Avg Frame (ms) | Avg FPS | Avg Build CPU (ms) | Frames |");
            sb.AppendLine("|-------|---------------|---------|-------------------|--------|");
            for (int i = 0; i < 4; i++)
            {
                if (phaseCount[i] == 0) continue;
                float pAvgFrame = phaseFrameSum[i] / phaseCount[i];
                float pAvgBuild = phaseBuildSum[i] / phaseCount[i];
                sb.AppendLine($"| {phaseNames[i]} | {pAvgFrame:F2} | {(1000f / pAvgFrame):F1} | {pAvgBuild:F4} | {phaseCount[i]} |");
            }

            sb.AppendLine();
            sb.AppendLine("## Screenshots");
            foreach (float t in screenshotTimes)
            {
                sb.AppendLine($"- sky_{t:F1}h.png");
            }

            File.WriteAllText(Path.Combine(outputDir, "report.md"), sb.ToString());

            // Also write raw CSV
            var csv = new StringBuilder();
            csv.AppendLine("timeOfDay,frameMs,cpuBuildMs");
            foreach (var s in samples)
            {
                csv.AppendLine($"{s.timeOfDay:F3},{s.frameMs:F3},{s.cpuBuildMs:F4}");
            }
            File.WriteAllText(Path.Combine(outputDir, "frames.csv"), csv.ToString());
        }

        static int GetPhase(float tod)
        {
            if (tod >= 5f && tod < 7f) return 0;   // dawn
            if (tod >= 7f && tod < 18f) return 1;   // day
            if (tod >= 18f && tod < 21f) return 2;  // dusk
            return 3;                                 // night
        }

        void OnGUI()
        {
            if (!isRunning) return;
            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
            float tod = cycle != null ? cycle.TimeOfDay : 0;
            GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30),
                $"BENCHMARK: {tod:F1}h ({samples.Count} frames)");
        }
    }
}
