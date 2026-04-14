using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace GTA5Sky.Tests
{
    [InitializeOnLoad]
    public static class AutoTestRunner
    {
        static readonly string TriggerFile = "E:/ShaderUnityTest/run-tests.trigger";
        static readonly string OutputDir = "E:/ShaderUnityTest/test-output";

        static AutoTestRunner()
        {
            if (!File.Exists(TriggerFile)) return;
            File.Delete(TriggerFile);
            Directory.CreateDirectory(OutputDir);

            EditorApplication.delayCall += () =>
            {
                try
                {
                    UnityEngine.Debug.Log("[AutoTestRunner] Trigger detected. Running...");
                    var sb = new StringBuilder();
                    sb.AppendLine("# Sky System Test & Performance Report");
                    sb.AppendLine($"Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"Unity: {Application.unityVersion}");
                    sb.AppendLine();

                    RunLogicTests(sb);
                    sb.AppendLine();
                    RunPerformanceBenchmark(sb);

                    string path = Path.Combine(OutputDir, "report.md");
                    File.WriteAllText(path, sb.ToString());
                    UnityEngine.Debug.Log($"[AutoTestRunner] Report saved: {path}");
                }
                catch (System.Exception ex)
                {
                    string errPath = Path.Combine(OutputDir, "error.txt");
                    File.WriteAllText(errPath, ex.ToString());
                    UnityEngine.Debug.LogError($"[AutoTestRunner] Failed: {ex.Message}");
                }
            };
        }

        static void RunLogicTests(StringBuilder sb)
        {
            sb.AppendLine("## Logic Tests");
            sb.AppendLine();

            var settings = WeatherSettings.CreateRuntimeDefaults();
            var clearProfile = settings.GetProfileOrDefault(WeatherType.Clear);
            var clearState = WeatherTransition.WeatherState.FromProfile(clearProfile);
            var rainyProfile = settings.GetProfileOrDefault(WeatherType.Rainy);
            var rainyState = WeatherTransition.WeatherState.FromProfile(rainyProfile);

            int passed = 0, failed = 0;

            // Test 1: Determinism
            {
                var a = GTA5TimecycleSky.Build(clearState, 12f, 100f);
                var b = GTA5TimecycleSky.Build(clearState, 12f, 100f);
                bool ok = a.SunDirection == b.SunDirection && a.SkyHdrIntensity == b.SkyHdrIntensity;
                Log(sb, "Deterministic (same input → same output)", ok, ref passed, ref failed);
            }

            // Test 2: Noon sun above horizon
            {
                var snap = GTA5TimecycleSky.Build(clearState, 12f, 0f);
                bool ok = snap.SunDirection.y > 0f && snap.SunFade > 0.9f && snap.MoonFade < 0.01f;
                Log(sb, "Noon: sun above horizon, sunFade~1, moonFade~0", ok, ref passed, ref failed,
                    $"sunDir.y={snap.SunDirection.y:F3} sunFade={snap.SunFade:F3} moonFade={snap.MoonFade:F3}");
            }

            // Test 3: Midnight sun below horizon
            {
                var snap = GTA5TimecycleSky.Build(clearState, 0f, 0f);
                bool ok = snap.SunDirection.y < 0f && snap.MoonFade > 0.5f;
                Log(sb, "Midnight: sun below horizon, moonFade > 0.5", ok, ref passed, ref failed,
                    $"sunDir.y={snap.SunDirection.y:F3} moonFade={snap.MoonFade:F3}");
            }

            // Test 4: DayNightBalance range [0,1] for all hours
            {
                bool ok = true;
                string failHour = "";
                for (float h = 0f; h < 24f; h += 0.5f)
                {
                    var snap = GTA5TimecycleSky.Build(clearState, h, 0f);
                    if (snap.DayNightBalance < 0f || snap.DayNightBalance > 1f)
                    {
                        ok = false; failHour = $"h={h} val={snap.DayNightBalance}"; break;
                    }
                }
                Log(sb, "DayNightBalance ∈ [0,1] for all hours", ok, ref passed, ref failed, failHour);
            }

            // Test 5: All colors non-negative
            {
                bool ok = true;
                string failInfo = "";
                for (float h = 0f; h < 24f; h += 1f)
                {
                    var snap = GTA5TimecycleSky.Build(clearState, h, 0f);
                    if (snap.AzimuthEastColor.r < 0 || snap.AzimuthEastColor.g < 0 || snap.AzimuthEastColor.b < 0 ||
                        snap.ZenithColor.r < 0 || snap.ZenithColor.g < 0 || snap.ZenithColor.b < 0 ||
                        snap.SkyHdrIntensity < 0)
                    {
                        ok = false; failInfo = $"Negative color at h={h}"; break;
                    }
                }
                Log(sb, "All colors non-negative across 24h", ok, ref passed, ref failed, failInfo);
            }

            // Test 6: Weather lerp correctness
            {
                var mid = WeatherTransition.WeatherState.Lerp(clearState, rainyState, 0.5f);
                float expected = Mathf.Lerp(clearState.FogDensity, rainyState.FogDensity, 0.5f);
                bool ok = Mathf.Abs(mid.FogDensity - expected) < 0.0001f;
                Log(sb, "WeatherState.Lerp midpoint correctness", ok, ref passed, ref failed,
                    $"expected={expected:F4} got={mid.FogDensity:F4}");
            }

            // Test 7: Sunrise/sunset transitions smooth (no discontinuity)
            {
                bool ok = true;
                string failInfo = "";
                float prevHdr = -1;
                for (float h = 4f; h < 8f; h += 0.05f)
                {
                    var snap = GTA5TimecycleSky.Build(clearState, h, 0f);
                    if (prevHdr >= 0 && Mathf.Abs(snap.SkyHdrIntensity - prevHdr) > 0.15f)
                    {
                        ok = false; failInfo = $"HDR jump at h={h:F2}: {prevHdr:F3}→{snap.SkyHdrIntensity:F3}"; break;
                    }
                    prevHdr = snap.SkyHdrIntensity;
                }
                Log(sb, "Sunrise HDR transition smooth (no jumps > 0.15)", ok, ref passed, ref failed, failInfo);
            }

            sb.AppendLine();
            sb.AppendLine($"**Total: {passed + failed} | Passed: {passed} | Failed: {failed}**");
        }

        static void RunPerformanceBenchmark(StringBuilder sb)
        {
            sb.AppendLine("## Performance Benchmark");
            sb.AppendLine();

            var settings = WeatherSettings.CreateRuntimeDefaults();
            var clearState = WeatherTransition.WeatherState.FromProfile(settings.GetProfileOrDefault(WeatherType.Clear));
            var rainyState = WeatherTransition.WeatherState.FromProfile(settings.GetProfileOrDefault(WeatherType.Rainy));

            var sw = new Stopwatch();
            const int N = 5000;

            // Warm up
            for (int i = 0; i < 100; i++)
                GTA5TimecycleSky.Build(clearState, i * 0.24f % 24f, i);

            // Build benchmark
            sb.AppendLine("### GTA5TimecycleSky.Build()");
            sw.Restart();
            for (int i = 0; i < N; i++)
                GTA5TimecycleSky.Build(clearState, (i * 0.0048f) % 24f, i * 0.02f);
            sw.Stop();
            double clearUs = sw.Elapsed.TotalMilliseconds / N * 1000.0;

            sw.Restart();
            for (int i = 0; i < N; i++)
                GTA5TimecycleSky.Build(rainyState, (i * 0.0048f) % 24f, i * 0.02f);
            sw.Stop();
            double rainyUs = sw.Elapsed.TotalMilliseconds / N * 1000.0;

            sb.AppendLine($"- Clear weather: **{clearUs:F1}us** per call ({N} iterations)");
            sb.AppendLine($"- Rainy weather: **{rainyUs:F1}us** per call ({N} iterations)");
            sb.AppendLine($"- % of 60fps frame (16.6ms): **{(clearUs / 16667.0 * 100):F4}%**");
            sb.AppendLine();

            // Per-phase breakdown
            sb.AppendLine("### Per-Phase Breakdown (Clear)");
            sb.AppendLine("| Phase | Hours | Avg (us) |");
            sb.AppendLine("|-------|-------|----------|");

            string[] names = { "Night", "Dawn", "Morning", "Noon", "Afternoon", "Dusk" };
            float[] lo = { 0, 5, 7, 11, 14, 18 };
            float[] hi = { 5, 7, 11, 14, 18, 21 };

            for (int p = 0; p < names.Length; p++)
            {
                sw.Restart();
                for (int i = 0; i < 1000; i++)
                {
                    float t = lo[p] + (hi[p] - lo[p]) * (i / 999f);
                    GTA5TimecycleSky.Build(clearState, t, i * 0.02f);
                }
                sw.Stop();
                double avgUs = sw.Elapsed.TotalMilliseconds / 1000.0 * 1000.0;
                sb.AppendLine($"| {names[p]} | {lo[p]:F0}-{hi[p]:F0}h | {avgUs:F1} |");
            }

            // Lerp benchmark
            sb.AppendLine();
            sb.AppendLine("### WeatherState.Lerp()");
            sw.Restart();
            for (int i = 0; i < N; i++)
                WeatherTransition.WeatherState.Lerp(clearState, rainyState, i / (float)N);
            sw.Stop();
            double lerpUs = sw.Elapsed.TotalMilliseconds / N * 1000.0;
            sb.AppendLine($"- Avg: **{lerpUs:F1}us** per call");

            sb.AppendLine();
            sb.AppendLine("### Summary");
            sb.AppendLine($"- Build() cost per frame at 60fps: **{(clearUs / 16667.0 * 100):F4}%**");
            sb.AppendLine($"- Lerp() cost per frame at 60fps: **{(lerpUs / 16667.0 * 100):F4}%**");
            sb.AppendLine($"- Combined sky CPU overhead: **~{((clearUs + lerpUs) / 16667.0 * 100):F3}%** of frame budget");
        }

        static void Log(StringBuilder sb, string name, bool pass, ref int passed, ref int failed, string detail = "")
        {
            string icon = pass ? "PASS" : "FAIL";
            sb.AppendLine($"- [{icon}] {name}");
            if (!string.IsNullOrEmpty(detail))
                sb.AppendLine($"  - {detail}");
            if (pass) passed++; else failed++;
        }
    }
}
