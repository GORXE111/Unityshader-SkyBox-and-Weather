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

            meshFilter.sharedMesh = CreateSphereMesh(1f, 64, 32);

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

            skyMaterial.SetColor("_AzimuthEastColor", p.azimuthEastColor);
            skyMaterial.SetFloat("_AzimuthEastIntensity", p.azimuthEastIntensity);
            skyMaterial.SetColor("_AzimuthWestColor", p.azimuthWestColor);
            skyMaterial.SetFloat("_AzimuthWestIntensity", p.azimuthWestIntensity);
            skyMaterial.SetColor("_AzimuthTransitionColor", p.azimuthTransitionColor);
            skyMaterial.SetFloat("_AzimuthTransitionIntensity", p.azimuthTransitionIntensity);
            skyMaterial.SetFloat("_AzimuthTransitionPos", p.azimuthTransitionPos);

            skyMaterial.SetColor("_ZenithColor", p.zenithColor);
            skyMaterial.SetFloat("_ZenithIntensity", p.zenithIntensity);
            skyMaterial.SetColor("_ZenithTransitionColor", p.zenithTransitionColor);
            skyMaterial.SetFloat("_ZenithTransitionIntensity", p.zenithTransitionIntensity);
            skyMaterial.SetFloat("_ZenithTransitionPos", p.zenithTransitionPos);
            skyMaterial.SetFloat("_ZenithTransEastBlend", p.zenithTransEastBlend);
            skyMaterial.SetFloat("_ZenithTransWestBlend", p.zenithTransWestBlend);
            skyMaterial.SetFloat("_ZenithBlendStart", p.zenithBlendStart);
            skyMaterial.SetFloat("_SkyHdrIntensity", p.skyHdrIntensity);
            skyMaterial.SetColor("_SkyPlaneColor", p.skyPlaneColor);
            skyMaterial.SetFloat("_SkyPlaneIntensity", p.skyPlaneIntensity);

            skyMaterial.SetVector("_SunDirection", p.sunDirection);
            skyMaterial.SetColor("_SunColorHdr", p.sunColorHdr);
            skyMaterial.SetColor("_SunDiscColor", p.sunDiscColor);
            skyMaterial.SetFloat("_SunDiscSize", p.sunDiscSize);
            skyMaterial.SetFloat("_SunHdrIntensity", p.sunHdrIntensity);
            skyMaterial.SetFloat("_MiePhase2", p.miePhase * 2f);
            skyMaterial.SetFloat("_MiePhaseSqr1", (p.miePhase * p.miePhase) + 1f);
            skyMaterial.SetFloat("_MieScatter", p.mieScatter);
            skyMaterial.SetFloat("_MieIntensity", p.mieIntensity);
            skyMaterial.SetFloat("_SunInfluenceRadius", p.sunInfluenceRadius);
            skyMaterial.SetFloat("_SunScatterIntensity", p.sunScatterIntensity);
            skyMaterial.SetFloat("_SunFade", p.sunFade);

            skyMaterial.SetVector("_MoonDirection", p.moonDirection);
            skyMaterial.SetColor("_MoonColor", p.moonColor);
            skyMaterial.SetFloat("_MoonDiscSize", p.moonDiscSize);
            skyMaterial.SetFloat("_MoonIntensity", p.moonIntensity);
            skyMaterial.SetFloat("_MoonInfluenceRadius", p.moonInfluenceRadius);
            skyMaterial.SetFloat("_MoonScatterIntensity", p.moonScatterIntensity);
            skyMaterial.SetFloat("_MoonPhaseOffset", p.moonPhaseOffset);
            skyMaterial.SetFloat("_MoonFade", p.moonFade);
            skyMaterial.SetFloat("_StarfieldIntensity", p.starfieldIntensity);

            skyMaterial.SetColor("_CloudBaseColor", p.cloudBaseColor);
            skyMaterial.SetColor("_CloudMidColor", p.cloudMidColor);
            skyMaterial.SetColor("_CloudShadowColor", p.cloudShadowColor);
            skyMaterial.SetFloat("_CloudBaseStrength", p.cloudBaseStrength);
            skyMaterial.SetFloat("_CloudDensityMultiplier", p.cloudDensityMultiplier);
            skyMaterial.SetFloat("_CloudDensityBias", p.cloudDensityBias);
            skyMaterial.SetFloat("_CloudEdgeStrength", p.cloudEdgeStrength);
            skyMaterial.SetFloat("_CloudOverallStrength", p.cloudOverallStrength);
            skyMaterial.SetFloat("_CloudFadeOut", p.cloudFadeOut);
            skyMaterial.SetFloat("_CloudHdrIntensity", p.cloudHdrIntensity);
            skyMaterial.SetFloat("_CloudOffset", p.cloudOffset);

            skyMaterial.SetColor("_SmallCloudColor", p.smallCloudColor);
            skyMaterial.SetFloat("_SmallCloudDetailStrength", p.smallCloudDetailStrength);
            skyMaterial.SetFloat("_SmallCloudDetailScale", p.smallCloudDetailScale);
            skyMaterial.SetFloat("_SmallCloudDensityMultiplier", p.smallCloudDensityMultiplier);
            skyMaterial.SetFloat("_SmallCloudDensityBias", p.smallCloudDensityBias);

            skyMaterial.SetFloat("_NoiseFrequency", p.noiseFrequency);
            skyMaterial.SetFloat("_NoiseScale", p.noiseScale);
            skyMaterial.SetFloat("_NoiseThreshold", p.noiseThreshold);
            skyMaterial.SetFloat("_NoiseSoftness", p.noiseSoftness);
            skyMaterial.SetFloat("_NoiseDensityOffset", p.noiseDensityOffset);
        }

        static Mesh CreateSphereMesh(float radius, int latSegments, int lonSegments)
        {
            Mesh mesh = new Mesh { name = "SkyDome" };
            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = Mathf.PI * lat / latSegments;
                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = 2f * Mathf.PI * lon / lonSegments;
                    vertices.Add(new Vector3(
                        Mathf.Sin(theta) * Mathf.Cos(phi),
                        Mathf.Cos(theta),
                        Mathf.Sin(theta) * Mathf.Sin(phi)) * radius);
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = lat * (lonSegments + 1) + lon;
                    int next = current + lonSegments + 1;
                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
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
            public float moonPhaseOffset;
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
