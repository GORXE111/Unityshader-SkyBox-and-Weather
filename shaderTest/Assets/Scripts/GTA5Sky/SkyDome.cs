using UnityEngine;
using UnityEngine.Rendering;

namespace GTA5Sky
{
    public sealed class SkyDome : MonoBehaviour
    {
        public static SkyDome Instance { get; private set; }

        MeshRenderer meshRenderer;
        Material skyMaterial;
        Camera targetCamera;

        // OPT: cached property IDs — avoids string lookup every frame
        static readonly int ID_AzimuthEastColor = Shader.PropertyToID("_AzimuthEastColor");
        static readonly int ID_AzimuthEastIntensity = Shader.PropertyToID("_AzimuthEastIntensity");
        static readonly int ID_AzimuthWestColor = Shader.PropertyToID("_AzimuthWestColor");
        static readonly int ID_AzimuthWestIntensity = Shader.PropertyToID("_AzimuthWestIntensity");
        static readonly int ID_AzimuthTransitionColor = Shader.PropertyToID("_AzimuthTransitionColor");
        static readonly int ID_AzimuthTransitionIntensity = Shader.PropertyToID("_AzimuthTransitionIntensity");
        static readonly int ID_AzimuthTransitionPos = Shader.PropertyToID("_AzimuthTransitionPos");
        static readonly int ID_ZenithColor = Shader.PropertyToID("_ZenithColor");
        static readonly int ID_ZenithIntensity = Shader.PropertyToID("_ZenithIntensity");
        static readonly int ID_ZenithTransitionColor = Shader.PropertyToID("_ZenithTransitionColor");
        static readonly int ID_ZenithTransitionIntensity = Shader.PropertyToID("_ZenithTransitionIntensity");
        static readonly int ID_ZenithTransitionPos = Shader.PropertyToID("_ZenithTransitionPos");
        static readonly int ID_ZenithTransEastBlend = Shader.PropertyToID("_ZenithTransEastBlend");
        static readonly int ID_ZenithTransWestBlend = Shader.PropertyToID("_ZenithTransWestBlend");
        static readonly int ID_ZenithBlendStart = Shader.PropertyToID("_ZenithBlendStart");
        static readonly int ID_SkyHdrIntensity = Shader.PropertyToID("_SkyHdrIntensity");
        static readonly int ID_SkyPlaneColor = Shader.PropertyToID("_SkyPlaneColor");
        static readonly int ID_SkyPlaneIntensity = Shader.PropertyToID("_SkyPlaneIntensity");
        static readonly int ID_SunDirection = Shader.PropertyToID("_SunDirection");
        static readonly int ID_SunColorHdr = Shader.PropertyToID("_SunColorHdr");
        static readonly int ID_SunDiscColor = Shader.PropertyToID("_SunDiscColor");
        static readonly int ID_SunDiscSize = Shader.PropertyToID("_SunDiscSize");
        static readonly int ID_SunHdrIntensity = Shader.PropertyToID("_SunHdrIntensity");
        static readonly int ID_MiePhase2 = Shader.PropertyToID("_MiePhase2");
        static readonly int ID_MiePhaseSqr1 = Shader.PropertyToID("_MiePhaseSqr1");
        static readonly int ID_MieScatter = Shader.PropertyToID("_MieScatter");
        static readonly int ID_MieIntensity = Shader.PropertyToID("_MieIntensity");
        static readonly int ID_SunInfluenceRadius = Shader.PropertyToID("_SunInfluenceRadius");
        static readonly int ID_SunScatterIntensity = Shader.PropertyToID("_SunScatterIntensity");
        static readonly int ID_SunFade = Shader.PropertyToID("_SunFade");
        static readonly int ID_MoonDirection = Shader.PropertyToID("_MoonDirection");
        static readonly int ID_MoonColor = Shader.PropertyToID("_MoonColor");
        static readonly int ID_MoonDiscSize = Shader.PropertyToID("_MoonDiscSize");
        static readonly int ID_MoonIntensity = Shader.PropertyToID("_MoonIntensity");
        static readonly int ID_MoonInfluenceRadius = Shader.PropertyToID("_MoonInfluenceRadius");
        static readonly int ID_MoonScatterIntensity = Shader.PropertyToID("_MoonScatterIntensity");
        static readonly int ID_MoonFade = Shader.PropertyToID("_MoonFade");
        static readonly int ID_StarfieldIntensity = Shader.PropertyToID("_StarfieldIntensity");
        static readonly int ID_CloudBaseColor = Shader.PropertyToID("_CloudBaseColor");
        static readonly int ID_CloudMidColor = Shader.PropertyToID("_CloudMidColor");
        static readonly int ID_CloudShadowColor = Shader.PropertyToID("_CloudShadowColor");
        static readonly int ID_CloudBaseStrength = Shader.PropertyToID("_CloudBaseStrength");
        static readonly int ID_CloudDensityMultiplier = Shader.PropertyToID("_CloudDensityMultiplier");
        static readonly int ID_CloudDensityBias = Shader.PropertyToID("_CloudDensityBias");
        static readonly int ID_CloudEdgeStrength = Shader.PropertyToID("_CloudEdgeStrength");
        static readonly int ID_CloudOverallStrength = Shader.PropertyToID("_CloudOverallStrength");
        static readonly int ID_CloudFadeOut = Shader.PropertyToID("_CloudFadeOut");
        static readonly int ID_CloudHdrIntensity = Shader.PropertyToID("_CloudHdrIntensity");
        static readonly int ID_CloudOffset = Shader.PropertyToID("_CloudOffset");
        static readonly int ID_SmallCloudColor = Shader.PropertyToID("_SmallCloudColor");
        static readonly int ID_SmallCloudDetailStrength = Shader.PropertyToID("_SmallCloudDetailStrength");
        static readonly int ID_SmallCloudDetailScale = Shader.PropertyToID("_SmallCloudDetailScale");
        static readonly int ID_SmallCloudDensityMultiplier = Shader.PropertyToID("_SmallCloudDensityMultiplier");
        static readonly int ID_SmallCloudDensityBias = Shader.PropertyToID("_SmallCloudDensityBias");
        static readonly int ID_NoiseFrequency = Shader.PropertyToID("_NoiseFrequency");
        static readonly int ID_NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int ID_NoiseThreshold = Shader.PropertyToID("_NoiseThreshold");
        static readonly int ID_NoiseSoftness = Shader.PropertyToID("_NoiseSoftness");
        static readonly int ID_NoiseDensityOffset = Shader.PropertyToID("_NoiseDensityOffset");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<SkyDome>() != null)
            {
                return;
            }

            GameObject go = new GameObject("SkyDome");
            go.AddComponent<SkyDome>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildDome();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
                }
            }

            if (targetCamera == null)
            {
                return;
            }

            transform.position = targetCamera.transform.position;
            float domeRadius = Mathf.Clamp(targetCamera.farClipPlane * 0.8f, 200f, 5000f);
            transform.localScale = Vector3.one * domeRadius;
        }

        void BuildDome()
        {
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // OPT: reduced from 64x32 (4096 tris) to 24x16 (768 tris) — sky has no detail geometry
            meshFilter.sharedMesh = CreateSphereMesh(1f, 24, 16);

            Shader shader = Shader.Find("GTA5Sky/Sky");
            if (shader == null)
            {
                Debug.LogError("[SkyDome] Missing shader GTA5Sky/Sky");
                enabled = false;
                return;
            }

            skyMaterial = new Material(shader)
            {
                name = "GTA5SkyMaterial",
                hideFlags = HideFlags.DontSave
            };
            skyMaterial.SetTexture("_StarTex", GTA5StarfieldTexture.GetOrCreate());

            // OPT: bind noise texture for GPU-side FBM (3 tex samples vs 24 ALU hash ops)
            Texture2D noiseTex = NoiseTextureGenerator.GetOrCreate();
            skyMaterial.SetTexture(Shader.PropertyToID("_NoiseTex"), noiseTex);
            skyMaterial.EnableKeyword("_NOISE_TEXTURE");

            meshRenderer.sharedMaterial = skyMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            gameObject.layer = 0;
        }

        public void SetSkyParams(SkyParams p)
        {
            if (skyMaterial == null)
            {
                return;
            }

            // OPT: all calls use cached int IDs instead of string lookups
            skyMaterial.SetColor(ID_AzimuthEastColor, p.azimuthEastColor);
            skyMaterial.SetFloat(ID_AzimuthEastIntensity, p.azimuthEastIntensity);
            skyMaterial.SetColor(ID_AzimuthWestColor, p.azimuthWestColor);
            skyMaterial.SetFloat(ID_AzimuthWestIntensity, p.azimuthWestIntensity);
            skyMaterial.SetColor(ID_AzimuthTransitionColor, p.azimuthTransitionColor);
            skyMaterial.SetFloat(ID_AzimuthTransitionIntensity, p.azimuthTransitionIntensity);
            skyMaterial.SetFloat(ID_AzimuthTransitionPos, p.azimuthTransitionPos);

            skyMaterial.SetColor(ID_ZenithColor, p.zenithColor);
            skyMaterial.SetFloat(ID_ZenithIntensity, p.zenithIntensity);
            skyMaterial.SetColor(ID_ZenithTransitionColor, p.zenithTransitionColor);
            skyMaterial.SetFloat(ID_ZenithTransitionIntensity, p.zenithTransitionIntensity);
            skyMaterial.SetFloat(ID_ZenithTransitionPos, p.zenithTransitionPos);
            skyMaterial.SetFloat(ID_ZenithTransEastBlend, p.zenithTransEastBlend);
            skyMaterial.SetFloat(ID_ZenithTransWestBlend, p.zenithTransWestBlend);
            skyMaterial.SetFloat(ID_ZenithBlendStart, p.zenithBlendStart);
            skyMaterial.SetFloat(ID_SkyHdrIntensity, p.skyHdrIntensity);
            skyMaterial.SetColor(ID_SkyPlaneColor, p.skyPlaneColor);
            skyMaterial.SetFloat(ID_SkyPlaneIntensity, p.skyPlaneIntensity);

            skyMaterial.SetVector(ID_SunDirection, p.sunDirection);
            skyMaterial.SetColor(ID_SunColorHdr, p.sunColorHdr);
            skyMaterial.SetColor(ID_SunDiscColor, p.sunDiscColor);
            skyMaterial.SetFloat(ID_SunDiscSize, p.sunDiscSize);
            skyMaterial.SetFloat(ID_SunHdrIntensity, p.sunHdrIntensity);
            skyMaterial.SetFloat(ID_MiePhase2, p.miePhase * 2f);
            skyMaterial.SetFloat(ID_MiePhaseSqr1, (p.miePhase * p.miePhase) + 1f);
            skyMaterial.SetFloat(ID_MieScatter, p.mieScatter);
            skyMaterial.SetFloat(ID_MieIntensity, p.mieIntensity);
            skyMaterial.SetFloat(ID_SunInfluenceRadius, p.sunInfluenceRadius);
            skyMaterial.SetFloat(ID_SunScatterIntensity, p.sunScatterIntensity);
            skyMaterial.SetFloat(ID_SunFade, p.sunFade);

            skyMaterial.SetVector(ID_MoonDirection, p.moonDirection);
            skyMaterial.SetColor(ID_MoonColor, p.moonColor);
            skyMaterial.SetFloat(ID_MoonDiscSize, p.moonDiscSize);
            skyMaterial.SetFloat(ID_MoonIntensity, p.moonIntensity);
            skyMaterial.SetFloat(ID_MoonInfluenceRadius, p.moonInfluenceRadius);
            skyMaterial.SetFloat(ID_MoonScatterIntensity, p.moonScatterIntensity);
            skyMaterial.SetFloat(ID_MoonFade, p.moonFade);
            skyMaterial.SetFloat(ID_StarfieldIntensity, p.starfieldIntensity);

            skyMaterial.SetColor(ID_CloudBaseColor, p.cloudBaseColor);
            skyMaterial.SetColor(ID_CloudMidColor, p.cloudMidColor);
            skyMaterial.SetColor(ID_CloudShadowColor, p.cloudShadowColor);
            skyMaterial.SetFloat(ID_CloudBaseStrength, p.cloudBaseStrength);
            skyMaterial.SetFloat(ID_CloudDensityMultiplier, p.cloudDensityMultiplier);
            skyMaterial.SetFloat(ID_CloudDensityBias, p.cloudDensityBias);
            skyMaterial.SetFloat(ID_CloudEdgeStrength, p.cloudEdgeStrength);
            skyMaterial.SetFloat(ID_CloudOverallStrength, p.cloudOverallStrength);
            skyMaterial.SetFloat(ID_CloudFadeOut, p.cloudFadeOut);
            skyMaterial.SetFloat(ID_CloudHdrIntensity, p.cloudHdrIntensity);
            skyMaterial.SetFloat(ID_CloudOffset, p.cloudOffset);

            skyMaterial.SetColor(ID_SmallCloudColor, p.smallCloudColor);
            skyMaterial.SetFloat(ID_SmallCloudDetailStrength, p.smallCloudDetailStrength);
            skyMaterial.SetFloat(ID_SmallCloudDetailScale, p.smallCloudDetailScale);
            skyMaterial.SetFloat(ID_SmallCloudDensityMultiplier, p.smallCloudDensityMultiplier);
            skyMaterial.SetFloat(ID_SmallCloudDensityBias, p.smallCloudDensityBias);

            skyMaterial.SetFloat(ID_NoiseFrequency, p.noiseFrequency);
            skyMaterial.SetFloat(ID_NoiseScale, p.noiseScale);
            skyMaterial.SetFloat(ID_NoiseThreshold, p.noiseThreshold);
            skyMaterial.SetFloat(ID_NoiseSoftness, p.noiseSoftness);
            skyMaterial.SetFloat(ID_NoiseDensityOffset, p.noiseDensityOffset);
        }

        static Mesh CreateSphereMesh(float radius, int latSegments, int lonSegments)
        {
            Mesh mesh = new Mesh { name = "SkyDome" };
            int vertCount = (latSegments + 1) * (lonSegments + 1);
            var vertices = new Vector3[vertCount];
            int idx = 0;

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = Mathf.PI * lat / latSegments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = 2f * Mathf.PI * lon / lonSegments;
                    vertices[idx++] = new Vector3(
                        sinTheta * Mathf.Cos(phi),
                        cosTheta,
                        sinTheta * Mathf.Sin(phi)) * radius;
                }
            }

            int triCount = latSegments * lonSegments * 6;
            var triangles = new int[triCount];
            idx = 0;

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = lat * (lonSegments + 1) + lon;
                    int next = current + lonSegments + 1;
                    triangles[idx++] = current;
                    triangles[idx++] = current + 1;
                    triangles[idx++] = next;
                    triangles[idx++] = next;
                    triangles[idx++] = current + 1;
                    triangles[idx++] = next + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius * 2f));
            return mesh;
        }

        public struct SkyParams
        {
            public Color azimuthEastColor;
            public float azimuthEastIntensity;
            public Color azimuthWestColor;
            public float azimuthWestIntensity;
            public Color azimuthTransitionColor;
            public float azimuthTransitionIntensity;
            public float azimuthTransitionPos;

            public Color zenithColor;
            public float zenithIntensity;
            public Color zenithTransitionColor;
            public float zenithTransitionIntensity;
            public float zenithTransitionPos;
            public float zenithTransEastBlend;
            public float zenithTransWestBlend;
            public float zenithBlendStart;
            public float skyHdrIntensity;
            public Color skyPlaneColor;
            public float skyPlaneIntensity;

            public Vector4 sunDirection;
            public Color sunColorHdr;
            public Color sunDiscColor;
            public float sunDiscSize;
            public float sunHdrIntensity;
            public float miePhase;
            public float mieScatter;
            public float mieIntensity;
            public float sunInfluenceRadius;
            public float sunScatterIntensity;
            public float sunFade;

            public Vector4 moonDirection;
            public Color moonColor;
            public float moonDiscSize;
            public float moonIntensity;
            public float moonInfluenceRadius;
            public float moonScatterIntensity;
            public float moonFade;
            public float starfieldIntensity;

            public Color cloudBaseColor;
            public Color cloudMidColor;
            public Color cloudShadowColor;
            public float cloudBaseStrength;
            public float cloudDensityMultiplier;
            public float cloudDensityBias;
            public float cloudEdgeStrength;
            public float cloudOverallStrength;
            public float cloudFadeOut;
            public float cloudHdrIntensity;
            public float cloudOffset;

            public Color smallCloudColor;
            public float smallCloudDetailStrength;
            public float smallCloudDetailScale;
            public float smallCloudDensityMultiplier;
            public float smallCloudDensityBias;

            public float noiseFrequency;
            public float noiseScale;
            public float noiseThreshold;
            public float noiseSoftness;
            public float noiseDensityOffset;
        }
    }
}
