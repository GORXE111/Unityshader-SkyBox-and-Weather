using UnityEngine;
using UnityEngine.Rendering;

namespace SkyPlanB
{
    /// <summary>
    /// Hillaire 2020 sky system controller.
    /// Dispatches compute shader to generate Sky View LUT each frame,
    /// manages sky dome, sun direction, and day/night cycle.
    /// </summary>
    public sealed class HillaireSkyController : MonoBehaviour
    {
        [Header("Sky View LUT")]
        [SerializeField] ComputeShader skyViewCompute;
        [SerializeField] int lutWidth = 192;
        [SerializeField] int lutHeight = 108;

        [Header("Textures")]
        [SerializeField] Texture2D transmittanceLUT;
        [SerializeField] Texture2D multiScatterLUT;
        [SerializeField] TextAsset multiScatterRawData; // .bytes file with float32 RGB

        Texture2D multiScatterFloat; // runtime float texture

        [Header("Sun")]
        [SerializeField] Light directionalLight;
        [SerializeField] Color sunColor = new Color(1f, 0.98f, 0.92f);
        [SerializeField] float sunIntensity = 30f;
        [SerializeField] float sunDiscSize = 0.9995f;

        [Header("Display")]
        [SerializeField] float exposure = 8f;
        [SerializeField] float cameraHeight = 0.001f; // km above ground

        [Header("Day/Night")]
        [SerializeField, Range(0f, 24f)] float timeOfDay = 12f;
        [SerializeField] float daySpeed = 0.1f;

        RenderTexture skyViewRT;
        Material skyMaterial;
        MeshFilter domeMeshFilter;
        MeshRenderer domeRenderer;
        Camera targetCamera;

        int kernelId;
        static readonly int ID_SkyViewLUT = Shader.PropertyToID("_SkyViewLUT");
        static readonly int ID_TransmittanceLUT = Shader.PropertyToID("_TransmittanceLUT");
        static readonly int ID_MultiScatterLUT = Shader.PropertyToID("_MultiScatterLUT");
        static readonly int ID_SunDirection = Shader.PropertyToID("_SunDirection");
        static readonly int ID_CameraHeight = Shader.PropertyToID("_CameraHeight");
        static readonly int ID_SkyViewSize = Shader.PropertyToID("_SkyViewSize");
        static readonly int ID_SunColor = Shader.PropertyToID("_SunColor");
        static readonly int ID_SunIntensity = Shader.PropertyToID("_SunIntensity");
        static readonly int ID_SunDiscSize = Shader.PropertyToID("_SunDiscSize");
        static readonly int ID_Exposure = Shader.PropertyToID("_Exposure");

        void Awake()
        {
            LoadMultiScatterFloat();
            CreateSkyViewRT();
            CreateDome();
        }

        void LoadMultiScatterFloat()
        {
            if (multiScatterRawData == null) return;

            byte[] raw = multiScatterRawData.bytes;
            int w = System.BitConverter.ToInt32(raw, 0);
            int h = System.BitConverter.ToInt32(raw, 4);
            int headerSize = 8;
            int pixelCount = w * h;

            multiScatterFloat = new Texture2D(w, h, TextureFormat.RGBAFloat, false)
            {
                name = "MultiScatterLUT_Float",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            var colors = new Color[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                int offset = headerSize + i * 12; // 3 floats × 4 bytes
                float r = System.BitConverter.ToSingle(raw, offset);
                float g = System.BitConverter.ToSingle(raw, offset + 4);
                float b = System.BitConverter.ToSingle(raw, offset + 8);
                colors[i] = new Color(r, g, b, 1f);
            }

            multiScatterFloat.SetPixels(colors);
            multiScatterFloat.Apply(false, true);
            Debug.Log($"[HillaireSky] Loaded float MultiScatter LUT: {w}x{h}");
        }

        void OnDestroy()
        {
            if (skyViewRT != null)
            {
                skyViewRT.Release();
                Destroy(skyViewRT);
            }
            if (skyMaterial != null) Destroy(skyMaterial);
            if (multiScatterFloat != null) Destroy(multiScatterFloat);
        }

        void Update()
        {
            // Day/night cycle
            if (!Mathf.Approximately(daySpeed, 0f))
            {
                timeOfDay = Mathf.Repeat(timeOfDay + daySpeed * Time.unscaledDeltaTime, 24f);
            }

            Vector3 sunDir = CalculateSunDirection(timeOfDay);

            // Update directional light
            if (directionalLight != null)
            {
                directionalLight.transform.rotation = Quaternion.LookRotation(-sunDir);
                directionalLight.color = sunColor;
                directionalLight.intensity = Mathf.Max(0.02f, sunDir.y);
            }

            // Dispatch compute shader
            DispatchSkyViewLUT(sunDir);

            // Update sky material
            if (skyMaterial != null)
            {
                skyMaterial.SetTexture(ID_SkyViewLUT, skyViewRT);
                skyMaterial.SetVector(ID_SunDirection, sunDir);
                skyMaterial.SetColor(ID_SunColor, sunColor);
                skyMaterial.SetFloat(ID_SunIntensity, sunIntensity);
                skyMaterial.SetFloat(ID_SunDiscSize, sunDiscSize);
                skyMaterial.SetFloat(ID_Exposure, exposure);
            }

            // Track camera
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null && domeMeshFilter != null)
            {
                transform.position = targetCamera.transform.position;
                float r = Mathf.Clamp(targetCamera.farClipPlane * 0.8f, 200f, 5000f);
                transform.localScale = Vector3.one * r;
            }
        }

        void CreateSkyViewRT()
        {
            skyViewRT = new RenderTexture(lutWidth, lutHeight, 0, RenderTextureFormat.ARGBHalf)
            {
                name = "SkyViewLUT",
                enableRandomWrite = true,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            skyViewRT.Create();

            if (skyViewCompute != null)
            {
                kernelId = skyViewCompute.FindKernel("CSMain");
            }
        }

        void CreateDome()
        {
            domeMeshFilter = gameObject.AddComponent<MeshFilter>();
            domeRenderer = gameObject.AddComponent<MeshRenderer>();

            domeMeshFilter.sharedMesh = CreateSphereMesh(1f, 24, 16);

            Shader shader = Shader.Find("SkyPlanB/HillaireSky");
            if (shader == null)
            {
                Debug.LogError("[HillaireSky] Shader not found");
                enabled = false;
                return;
            }

            skyMaterial = new Material(shader)
            {
                name = "HillaireSkyMaterial",
                hideFlags = HideFlags.DontSave
            };

            domeRenderer.sharedMaterial = skyMaterial;
            domeRenderer.shadowCastingMode = ShadowCastingMode.Off;
            domeRenderer.receiveShadows = false;
            domeRenderer.lightProbeUsage = LightProbeUsage.Off;
            domeRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            RenderSettings.skybox = null;
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
            }
        }

        void DispatchSkyViewLUT(Vector3 sunDir)
        {
            if (skyViewCompute == null || skyViewRT == null) return;

            skyViewCompute.SetTexture(kernelId, ID_SkyViewLUT, skyViewRT);

            if (transmittanceLUT != null)
                skyViewCompute.SetTexture(kernelId, ID_TransmittanceLUT, transmittanceLUT);
            // Prefer float texture, fallback to 8-bit PNG
            Texture msTexture = multiScatterFloat != null ? (Texture)multiScatterFloat : multiScatterLUT;
            if (msTexture != null)
                skyViewCompute.SetTexture(kernelId, ID_MultiScatterLUT, msTexture);

            skyViewCompute.SetVector(ID_SunDirection, sunDir);
            skyViewCompute.SetFloat(ID_CameraHeight, cameraHeight);
            skyViewCompute.SetVector(ID_SkyViewSize, new Vector4(lutWidth, lutHeight, 1f / lutWidth, 1f / lutHeight));

            int groupsX = Mathf.CeilToInt(lutWidth / 8f);
            int groupsY = Mathf.CeilToInt(lutHeight / 8f);
            skyViewCompute.Dispatch(kernelId, groupsX, groupsY, 1);
        }

        static Vector3 CalculateSunDirection(float hours)
        {
            float angle = (hours / 24f) * Mathf.PI * 2f - Mathf.PI * 0.5f;
            float elevation = Mathf.Sin(angle);
            float azimuth = Mathf.Cos(angle);
            // Tilt the orbit 55 degrees like GTA5 reference
            float tilt = 55f * Mathf.Deg2Rad;
            return new Vector3(
                azimuth * Mathf.Cos(tilt),
                elevation,
                azimuth * Mathf.Sin(tilt)
            ).normalized;
        }

        static Mesh CreateSphereMesh(float radius, int lat, int lon)
        {
            Mesh mesh = new Mesh { name = "SkyDomeB" };
            int vertCount = (lat + 1) * (lon + 1);
            var verts = new Vector3[vertCount];
            int idx = 0;
            for (int la = 0; la <= lat; la++)
            {
                float theta = Mathf.PI * la / lat;
                float st = Mathf.Sin(theta), ct = Mathf.Cos(theta);
                for (int lo = 0; lo <= lon; lo++)
                {
                    float phi = 2f * Mathf.PI * lo / lon;
                    verts[idx++] = new Vector3(st * Mathf.Cos(phi), ct, st * Mathf.Sin(phi)) * radius;
                }
            }
            int triCount = lat * lon * 6;
            var tris = new int[triCount];
            idx = 0;
            for (int la = 0; la < lat; la++)
                for (int lo = 0; lo < lon; lo++)
                {
                    int c = la * (lon + 1) + lo, n = c + lon + 1;
                    tris[idx++] = c; tris[idx++] = c + 1; tris[idx++] = n;
                    tris[idx++] = n; tris[idx++] = c + 1; tris[idx++] = n + 1;
                }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * radius * 2);
            return mesh;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 280, 160));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"[Plan B] Hillaire Sky — {timeOfDay:F2}h");
            float newTime = GUILayout.HorizontalSlider(timeOfDay, 0f, 24f);
            if (!Mathf.Approximately(newTime, timeOfDay)) timeOfDay = newTime;
            GUILayout.Label($"Speed: {daySpeed:F3}");
            daySpeed = GUILayout.HorizontalSlider(daySpeed, 0f, 2f);
            GUILayout.Label($"Exposure: {exposure:F1}");
            exposure = GUILayout.HorizontalSlider(exposure, 0.5f, 30f);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
