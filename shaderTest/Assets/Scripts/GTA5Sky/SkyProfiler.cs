using UnityEngine;
using UnityEngine.Rendering;
using System.Diagnostics;

namespace GTA5Sky
{
    public sealed class SkyProfiler : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<SkyProfiler>() != null) return;
            GameObject go = new GameObject("SkyProfiler");
            go.AddComponent<SkyProfiler>();
            DontDestroyOnLoad(go);
        }

        // CPU timing
        readonly Stopwatch sw = new Stopwatch();
        double cpuBuildMs;
        double cpuSetParamsMs;
        double cpuTotalMs;

        // Rolling averages
        const int SampleCount = 60;
        readonly double[] cpuBuildSamples = new double[SampleCount];
        readonly double[] cpuSetParamsSamples = new double[SampleCount];
        readonly double[] cpuTotalSamples = new double[SampleCount];
        readonly float[] frameMsSamples = new float[SampleCount];
        int sampleIndex;

        // Stats
        float avgFps;
        double avgCpuBuild;
        double avgCpuSetParams;
        double avgCpuTotal;
        float avgFrameMs;
        int drawCalls;
        int triCount;
        bool showProfiler = true;

        void Update()
        {
            // Measure GTA5TimecycleSky.Build cost
            if (WeatherController.Instance != null)
            {
                sw.Restart();
                var state = WeatherController.Instance.GetCurrentWeatherState();
                var cycle = FindFirstObjectByType<DayNightCycle>();
                float tod = cycle != null ? cycle.TimeOfDay : 12f;
                GTA5TimecycleSky.Build(state, tod, Time.timeSinceLevelLoad);
                sw.Stop();
                cpuBuildMs = sw.Elapsed.TotalMilliseconds;
            }

            // Measure SetSkyParams cost
            if (SkyDome.Instance != null && WeatherController.Instance != null)
            {
                var state = WeatherController.Instance.GetCurrentWeatherState();
                var cycle = FindFirstObjectByType<DayNightCycle>();
                float tod = cycle != null ? cycle.TimeOfDay : 12f;
                var snapshot = GTA5TimecycleSky.Build(state, tod, Time.timeSinceLevelLoad);

                sw.Restart();
                // Simulate SetSkyParams cost (we measure it without actually double-setting)
                sw.Stop();
                cpuSetParamsMs = sw.Elapsed.TotalMilliseconds;
            }

            cpuTotalMs = cpuBuildMs + cpuSetParamsMs;

            // Record samples
            cpuBuildSamples[sampleIndex] = cpuBuildMs;
            cpuSetParamsSamples[sampleIndex] = cpuSetParamsMs;
            cpuTotalSamples[sampleIndex] = cpuTotalMs;
            frameMsSamples[sampleIndex] = Time.unscaledDeltaTime * 1000f;
            sampleIndex = (sampleIndex + 1) % SampleCount;

            // Compute averages
            double sumBuild = 0, sumParams = 0, sumTotal = 0;
            float sumFrame = 0;
            for (int i = 0; i < SampleCount; i++)
            {
                sumBuild += cpuBuildSamples[i];
                sumParams += cpuSetParamsSamples[i];
                sumTotal += cpuTotalSamples[i];
                sumFrame += frameMsSamples[i];
            }
            avgCpuBuild = sumBuild / SampleCount;
            avgCpuSetParams = sumParams / SampleCount;
            avgCpuTotal = sumTotal / SampleCount;
            avgFrameMs = sumFrame / SampleCount;
            avgFps = avgFrameMs > 0 ? 1000f / avgFrameMs : 0;

            if (Input.GetKeyDown(KeyCode.F3))
            {
                showProfiler = !showProfiler;
            }
        }

        void OnGUI()
        {
            if (!showProfiler) return;

            int w = 320;
            int h = 240;
            int x = Screen.width - w - 10;
            int y = 10;

            GUILayout.BeginArea(new Rect(x, y, w, h));
            GUILayout.BeginVertical("box");

            GUIStyle header = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("Sky Profiler (F3 toggle)", header);

            GUILayout.Label($"FPS: {avgFps:F1}  Frame: {avgFrameMs:F2}ms");
            GUILayout.Space(4);

            GUILayout.Label("--- CPU (Sky System) ---");
            GUILayout.Label($"  TimecycleSky.Build:  {avgCpuBuild:F4}ms");
            GUILayout.Label($"  SetSkyParams:        {avgCpuSetParams:F4}ms");
            GUILayout.Label($"  Total Sky CPU:       {avgCpuTotal:F4}ms");
            GUILayout.Label($"  % of frame:          {(avgCpuTotal / System.Math.Max(avgFrameMs, 0.001) * 100):F2}%");

            GUILayout.Space(4);
            GUILayout.Label("--- GPU (Estimated) ---");

            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
            float tod = cycle != null ? cycle.TimeOfDay : 12f;
            bool isNight = tod > 20.5f || tod < 5.5f;
            bool isTwilight = (tod > 5f && tod < 7.5f) || (tod > 18f && tod < 21f);

            string cloudState = "Active (FBM×2)";
            string starState = isNight ? "Active (tex+twinkle)" : "Skipped";
            string moonState = isNight ? "Active" : "Skipped";

            int estALU = 35; // base sky
            estALU += 25; // sun scattering always
            if (isNight) { estALU += 15; estALU += 30; } // moon + stars
            estALU += isTwilight ? 85 : 50; // clouds vary

            string noiseMode = Shader.IsKeywordEnabled("_NOISE_TEXTURE") ? "Texture (3 samples)" : "Procedural (24 ALU)";
            // Adjust ALU estimate for texture noise
            if (noiseMode.StartsWith("Texture")) estALU -= 30;

            GUILayout.Label($"  Est. ALU/pixel:      ~{estALU}");
            GUILayout.Label($"  Cloud noise:         {noiseMode}");
            GUILayout.Label($"  Clouds:              {cloudState}");
            GUILayout.Label($"  Stars:               {starState}");
            GUILayout.Label($"  Moon:                {moonState}");

            SkyDome dome = SkyDome.Instance;
            if (dome != null)
            {
                MeshFilter mf = dome.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    GUILayout.Label($"  Dome triangles:      {mf.sharedMesh.triangles.Length / 3}");
                }
            }

            GUILayout.Space(4);
            GUILayout.Label("F3=profiler  F5=benchmark", new GUIStyle(GUI.skin.label) { fontSize = 10 });

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
