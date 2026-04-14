using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GTA5Sky
{
    /// <summary>
    /// Captures real Unity Profiler markers and exports to disk.
    /// Trigger: file or F7 key. Records 600 frames (~10 seconds at 60fps).
    /// </summary>
    public sealed class ProfilerDataExporter : MonoBehaviour
    {
        static readonly string ReadyFlag = "E:/ShaderUnityTest/profiler-export-ready.flag";
        static readonly string OutputDir = "E:/ShaderUnityTest/test-output";

        // Profiler recorders for real engine markers
        ProfilerRecorder mainThreadRecorder;
        ProfilerRecorder renderThreadRecorder;
        ProfilerRecorder cameraRenderRecorder;
        ProfilerRecorder drawCallsRecorder;
        ProfilerRecorder trianglesRecorder;
        ProfilerRecorder batchesRecorder;
        ProfilerRecorder setPassRecorder;
        ProfilerRecorder shadowCastersRecorder;
        ProfilerRecorder usedTexturesCountRecorder;
        ProfilerRecorder usedTexturesBytesRecorder;
        ProfilerRecorder renderTexturesCountRecorder;
        ProfilerRecorder verticesRecorder;

        bool isRunning;
        bool autoMode;

        struct ProfilerFrame
        {
            public float timeOfDay;
            public double mainThreadMs;
            public double renderThreadMs;
            public double cameraRenderMs;
            public long drawCalls;
            public long triangles;
            public long batches;
            public long setPassCalls;
            public long vertices;
            public long shadowCasters;
            public long usedTextures;
            public long usedTexturesBytes;
            public long renderTextures;
        }

        readonly List<ProfilerFrame> frames = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<ProfilerDataExporter>() != null) return;
            GameObject go = new GameObject("ProfilerDataExporter");
            go.AddComponent<ProfilerDataExporter>();
            DontDestroyOnLoad(go);
        }

        void OnEnable()
        {
            mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 1);
            cameraRenderRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Camera.Render", 1);
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPassRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
            shadowCastersRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
            usedTexturesCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Count");
            usedTexturesBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Bytes");
            renderTexturesCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Textures Count");
        }

        void OnDisable()
        {
            mainThreadRecorder.Dispose();
            renderThreadRecorder.Dispose();
            cameraRenderRecorder.Dispose();
            drawCallsRecorder.Dispose();
            trianglesRecorder.Dispose();
            batchesRecorder.Dispose();
            setPassRecorder.Dispose();
            verticesRecorder.Dispose();
            shadowCastersRecorder.Dispose();
            usedTexturesCountRecorder.Dispose();
            usedTexturesBytesRecorder.Dispose();
            renderTexturesCountRecorder.Dispose();
        }

        void Start()
        {
            if (File.Exists(ReadyFlag))
            {
                File.Delete(ReadyFlag);
                autoMode = true;
                StartCoroutine(Capture());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7) && !isRunning)
            {
                autoMode = false;
                StartCoroutine(Capture());
            }
        }

        IEnumerator Capture()
        {
            isRunning = true;
            frames.Clear();

            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();

            // Wait for recorders to stabilize
            for (int i = 0; i < 5; i++) yield return null;

            Debug.Log("[ProfilerExport] Capturing 600 frames...");

            for (int i = 0; i < 600; i++)
            {
                yield return new WaitForEndOfFrame();

                frames.Add(new ProfilerFrame
                {
                    timeOfDay = cycle != null ? cycle.TimeOfDay : 0,
                    mainThreadMs = GetRecorderMs(mainThreadRecorder),
                    renderThreadMs = GetRecorderMs(renderThreadRecorder),
                    cameraRenderMs = GetRecorderMs(cameraRenderRecorder),
                    drawCalls = drawCallsRecorder.LastValue,
                    triangles = trianglesRecorder.LastValue,
                    batches = batchesRecorder.LastValue,
                    setPassCalls = setPassRecorder.LastValue,
                    vertices = verticesRecorder.LastValue,
                    shadowCasters = shadowCastersRecorder.LastValue,
                    usedTextures = usedTexturesCountRecorder.LastValue,
                    usedTexturesBytes = usedTexturesBytesRecorder.LastValue,
                    renderTextures = renderTexturesCountRecorder.LastValue,
                });
            }

            GenerateReport();
            isRunning = false;

            #if UNITY_EDITOR
            if (autoMode)
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        static double GetRecorderMs(ProfilerRecorder recorder)
        {
            return recorder.Valid && recorder.LastValue > 0
                ? recorder.LastValue * 1e-6  // nanoseconds to milliseconds
                : 0;
        }

        void GenerateReport()
        {
            Directory.CreateDirectory(OutputDir);
            var sb = new StringBuilder();

            sb.AppendLine("# Unity Profiler Data Export");
            sb.AppendLine($"Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Resolution: {Screen.width}x{Screen.height}");
            sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"API: {SystemInfo.graphicsDeviceType}");
            sb.AppendLine($"Frames: {frames.Count}");
            sb.AppendLine();

            // Compute stats
            double sumMain = 0, sumRender = 0, sumCamera = 0;
            double maxMain = 0, maxRender = 0, maxCamera = 0;
            long sumDc = 0, sumTri = 0, sumBatch = 0, sumSetPass = 0;
            int validMain = 0, validRender = 0, validCamera = 0;

            foreach (var f in frames)
            {
                if (f.mainThreadMs > 0) { sumMain += f.mainThreadMs; validMain++; if (f.mainThreadMs > maxMain) maxMain = f.mainThreadMs; }
                if (f.renderThreadMs > 0) { sumRender += f.renderThreadMs; validRender++; if (f.renderThreadMs > maxRender) maxRender = f.renderThreadMs; }
                if (f.cameraRenderMs > 0) { sumCamera += f.cameraRenderMs; validCamera++; if (f.cameraRenderMs > maxCamera) maxCamera = f.cameraRenderMs; }
                sumDc += f.drawCalls;
                sumTri += f.triangles;
                sumBatch += f.batches;
                sumSetPass += f.setPassCalls;
            }

            sb.AppendLine("## Thread Timing");
            if (validMain > 0) sb.AppendLine($"- **Main Thread**: avg {sumMain / validMain:F3}ms, max {maxMain:F3}ms ({validMain} valid frames)");
            if (validRender > 0) sb.AppendLine($"- **Render Thread**: avg {sumRender / validRender:F3}ms, max {maxRender:F3}ms ({validRender} valid frames)");
            if (validCamera > 0) sb.AppendLine($"- **Camera.Render**: avg {sumCamera / validCamera:F3}ms, max {maxCamera:F3}ms ({validCamera} valid frames)");

            if (validMain > 0 && validRender > 0)
            {
                double avgMain = sumMain / validMain;
                double avgRender = sumRender / validRender;
                sb.AppendLine($"- **Bottleneck**: {(avgMain > avgRender ? "Main Thread" : "Render Thread")}");
                sb.AppendLine($"- **Render/Main ratio**: {avgRender / avgMain:F2}x");
            }
            sb.AppendLine();

            sb.AppendLine("## Rendering Stats (avg)");
            sb.AppendLine($"- Draw Calls: {sumDc / frames.Count}");
            sb.AppendLine($"- Triangles: {sumTri / frames.Count}");
            sb.AppendLine($"- Batches: {sumBatch / frames.Count}");
            sb.AppendLine($"- SetPass Calls: {sumSetPass / frames.Count}");
            if (frames.Count > 0)
            {
                var last = frames[frames.Count - 1];
                sb.AppendLine($"- Vertices: {last.vertices}");
                sb.AppendLine($"- Shadow Casters: {last.shadowCasters}");
                sb.AppendLine($"- Used Textures: {last.usedTextures} ({last.usedTexturesBytes / 1024}KB)");
                sb.AppendLine($"- Render Textures: {last.renderTextures}");
            }

            sb.AppendLine();
            sb.AppendLine("## Percentile Analysis (Main Thread)");
            if (validMain > 0)
            {
                var sorted = new List<double>();
                foreach (var f in frames) if (f.mainThreadMs > 0) sorted.Add(f.mainThreadMs);
                sorted.Sort();
                sb.AppendLine($"- P50: {sorted[(int)(sorted.Count * 0.50)]:F3}ms");
                sb.AppendLine($"- P90: {sorted[(int)(sorted.Count * 0.90)]:F3}ms");
                sb.AppendLine($"- P95: {sorted[(int)(sorted.Count * 0.95)]:F3}ms");
                sb.AppendLine($"- P99: {sorted[(int)(sorted.Count * 0.99)]:F3}ms");
            }

            sb.AppendLine();
            sb.AppendLine("## Percentile Analysis (Render Thread)");
            if (validRender > 0)
            {
                var sorted = new List<double>();
                foreach (var f in frames) if (f.renderThreadMs > 0) sorted.Add(f.renderThreadMs);
                sorted.Sort();
                sb.AppendLine($"- P50: {sorted[(int)(sorted.Count * 0.50)]:F3}ms");
                sb.AppendLine($"- P90: {sorted[(int)(sorted.Count * 0.90)]:F3}ms");
                sb.AppendLine($"- P95: {sorted[(int)(sorted.Count * 0.95)]:F3}ms");
                sb.AppendLine($"- P99: {sorted[(int)(sorted.Count * 0.99)]:F3}ms");
            }

            string path = Path.Combine(OutputDir, "profiler-export.md");
            File.WriteAllText(path, sb.ToString());

            // CSV
            var csv = new StringBuilder();
            csv.AppendLine("timeOfDay,mainThreadMs,renderThreadMs,cameraRenderMs,drawCalls,triangles,batches,setPassCalls");
            foreach (var f in frames)
                csv.AppendLine($"{f.timeOfDay:F3},{f.mainThreadMs:F4},{f.renderThreadMs:F4},{f.cameraRenderMs:F4},{f.drawCalls},{f.triangles},{f.batches},{f.setPassCalls}");
            File.WriteAllText(Path.Combine(OutputDir, "profiler-frames.csv"), csv.ToString());

            Debug.Log($"[ProfilerExport] Saved: {path}");
        }

        void OnGUI()
        {
            if (!isRunning) return;
            GUI.Label(new Rect(Screen.width / 2 - 100, 70, 200, 30),
                $"PROFILER CAPTURE: {frames.Count}/600");
        }
    }
}
