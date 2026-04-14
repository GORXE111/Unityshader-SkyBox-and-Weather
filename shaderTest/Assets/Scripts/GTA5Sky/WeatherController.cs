using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTA5Sky
{
    [DisallowMultipleComponent]
    public sealed class WeatherController : MonoBehaviour
    {
        public static WeatherController Instance { get; private set; }

        const float DefaultTransitionSeconds = 30f;

        [SerializeField] WeatherSettings weatherSettings;
        [SerializeField] WeatherType initialWeather = WeatherType.Clear;
        [SerializeField] float defaultTransitionDuration = DefaultTransitionSeconds;

        readonly WeatherTransition transition = new WeatherTransition();

        WeatherSettings runtimeSettings;
        DayNightCycle dayNightCycle;
        Light directionalLight;
        Volume globalVolume;
        VolumeProfile runtimeVolumeProfile;
        ColorAdjustments colorAdjustments;
        Tonemapping tonemapping;
        Bloom bloom;
        WeatherType activeWeather;
        WeatherType targetWeather;
        bool hasSolarOverride;
        bool isInitialized;
        float overrideSunAzimuth;
        float overrideSunZenith;


        public WeatherType CurrentWeather => activeWeather;
        public WeatherType TargetWeather => targetWeather;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (FindFirstObjectByType<WeatherController>() != null)
            {
                return;
            }

            GameObject go = new GameObject("WeatherSystem");
            go.AddComponent<WeatherController>();
            DontDestroyOnLoad(go);
        }

        public WeatherTransition.WeatherState GetCurrentWeatherState()
        {
            return GetEffectiveState(transition.CurrentState);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureDependencies();
            transition.Reset(ResolveState(initialWeather));
            activeWeather = initialWeather;
            targetWeather = initialWeather;
            isInitialized = true;

            if (dayNightCycle != null)
            {
                dayNightCycle.ApplyCurrentTime();
            }
            else
            {
                ApplyState(transition.CurrentState);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void Update()
        {
            if (!transition.Tick(Time.deltaTime))
            {
                return;
            }

            ApplyState(transition.CurrentState);
            if (!transition.IsTransitioning)
            {
                activeWeather = targetWeather;
            }
        }

        public void SetWeather(WeatherType nextWeather)
        {
            SetWeather(nextWeather, defaultTransitionDuration, false);
        }

        public void SetWeatherImmediate(WeatherType nextWeather)
        {
            SetWeather(nextWeather, 0f, true);
        }

        public void SetWeather(WeatherType nextWeather, float transitionDuration, bool immediate)
        {
            WeatherTransition.WeatherState nextState = ResolveState(nextWeather);
            float duration = immediate ? 0f : Mathf.Max(0f, transitionDuration);
            targetWeather = nextWeather;

            if (duration <= 0f)
            {
                transition.Reset(nextState);
                activeWeather = nextWeather;
                ApplyState(nextState);
            }
            else
            {
                transition.Begin(transition.CurrentState, nextState, duration);
            }
        }

        // OPT: track last applied values to skip redundant updates
        float lastAppliedAzimuth = -999f;
        float lastAppliedZenith = -999f;
        const float SolarThreshold = 0.01f;

        public void SetSolarParameters(float sunAzimuth, float sunZenith)
        {
            hasSolarOverride = true;
            overrideSunAzimuth = Mathf.Repeat(sunAzimuth, 360f);
            overrideSunZenith = Mathf.Clamp(sunZenith, -90f, 90f);

            // OPT: skip if solar params barely changed (< 0.01 degree)
            if (isInitialized &&
                (Mathf.Abs(overrideSunAzimuth - lastAppliedAzimuth) > SolarThreshold ||
                 Mathf.Abs(overrideSunZenith - lastAppliedZenith) > SolarThreshold))
            {
                lastAppliedAzimuth = overrideSunAzimuth;
                lastAppliedZenith = overrideSunZenith;
                ApplyState(transition.CurrentState);
            }
        }

        public void ForceRefreshSky()
        {
            if (!isInitialized)
            {
                return;
            }

            ApplyState(transition.CurrentState);
        }

        void EnsureDependencies()
        {
            if (!TryGetComponent(out dayNightCycle) || dayNightCycle == null)
            {
                dayNightCycle = gameObject.AddComponent<DayNightCycle>();
            }
        }

        WeatherTransition.WeatherState ResolveState(WeatherType weatherType)
        {
            WeatherSettings settings = ResolveSettings();
            WeatherSettings.WeatherProfile profile = settings.GetProfileOrDefault(weatherType);
            return WeatherTransition.WeatherState.FromProfile(profile);
        }

        WeatherSettings ResolveSettings()
        {
            if (weatherSettings != null)
            {
                return weatherSettings;
            }

            if (runtimeSettings == null)
            {
                runtimeSettings = WeatherSettings.CreateRuntimeDefaults();
            }

            return runtimeSettings;
        }

        void ApplyState(WeatherTransition.WeatherState state)
        {
            WeatherTransition.WeatherState effectiveState = GetEffectiveState(state);
            GTA5TimecycleSky.Snapshot snapshot = GTA5TimecycleSky.Build(effectiveState, ResolveTimeOfDay(), Time.timeSinceLevelLoad);

            ApplySky(snapshot);
            ApplyFog(effectiveState, snapshot);
            ApplyDirectionalLight(effectiveState, snapshot);
            ApplyPostProcessing(snapshot);

            RenderSettings.ambientLight = Color.Lerp(
                effectiveState.AmbientLight * 0.28f,
                effectiveState.AmbientLight,
                Mathf.Clamp01(snapshot.DayNightBalance + 0.2f));
        }

        void ApplySky(GTA5TimecycleSky.Snapshot snapshot)
        {
            SkyDome skyDome = SkyDome.Instance;
            if (skyDome != null)
            {
                skyDome.SetSkyParams(new SkyDome.SkyParams
                {
                    azimuthEastColor = snapshot.AzimuthEastColor,
                    azimuthEastIntensity = snapshot.AzimuthEastIntensity,
                    azimuthWestColor = snapshot.AzimuthWestColor,
                    azimuthWestIntensity = snapshot.AzimuthWestIntensity,
                    azimuthTransitionColor = snapshot.AzimuthTransitionColor,
                    azimuthTransitionIntensity = snapshot.AzimuthTransitionIntensity,
                    azimuthTransitionPos = snapshot.AzimuthTransitionPosition,
                    zenithColor = snapshot.ZenithColor,
                    zenithIntensity = snapshot.ZenithIntensity,
                    zenithTransitionColor = snapshot.ZenithTransitionColor,
                    zenithTransitionIntensity = snapshot.ZenithTransitionIntensity,
                    zenithTransitionPos = snapshot.ZenithTransitionPosition,
                    zenithTransEastBlend = snapshot.ZenithTransitionEastBlend,
                    zenithTransWestBlend = snapshot.ZenithTransitionWestBlend,
                    zenithBlendStart = snapshot.ZenithBlendStart,
                    skyHdrIntensity = snapshot.SkyHdrIntensity,
                    skyPlaneColor = snapshot.SkyPlaneColor,
                    skyPlaneIntensity = snapshot.SkyPlaneIntensity,
                    sunDirection = new Vector4(snapshot.SunDirection.x, snapshot.SunDirection.y, snapshot.SunDirection.z, 0f),
                    sunColorHdr = snapshot.SunColor,
                    sunDiscColor = snapshot.SunDiscColor,
                    sunDiscSize = snapshot.SunDiscSize,
                    sunHdrIntensity = snapshot.SunHdrIntensity,
                    miePhase = snapshot.MiePhase,
                    mieScatter = snapshot.MieScatter,
                    mieIntensity = snapshot.MieIntensity,
                    sunInfluenceRadius = snapshot.SunInfluenceRadius,
                    sunScatterIntensity = snapshot.SunScatterIntensity,
                    sunFade = snapshot.SunFade,
                    moonDirection = new Vector4(snapshot.MoonDirection.x, snapshot.MoonDirection.y, snapshot.MoonDirection.z, 0f),
                    moonColor = snapshot.MoonColor,
                    moonDiscSize = snapshot.MoonDiscSize,
                    moonIntensity = snapshot.MoonIntensity,
                    moonInfluenceRadius = snapshot.MoonInfluenceRadius,
                    moonScatterIntensity = snapshot.MoonScatterIntensity,
                    moonFade = snapshot.MoonFade,
                    starfieldIntensity = snapshot.StarfieldIntensity,
                    cloudBaseColor = snapshot.CloudBaseColor,
                    cloudMidColor = snapshot.CloudMidColor,
                    cloudShadowColor = snapshot.CloudShadowColor,
                    cloudBaseStrength = snapshot.CloudBaseStrength,
                    cloudDensityMultiplier = snapshot.CloudDensityMultiplier,
                    cloudDensityBias = snapshot.CloudDensityBias,
                    cloudEdgeStrength = snapshot.CloudEdgeStrength,
                    cloudOverallStrength = snapshot.CloudOverallStrength,
                    cloudFadeOut = snapshot.CloudFadeOut,
                    cloudHdrIntensity = snapshot.CloudHdrIntensity,
                    cloudOffset = snapshot.CloudOffset,
                    smallCloudColor = snapshot.SmallCloudColor,
                    smallCloudDetailStrength = snapshot.SmallCloudDetailStrength,
                    smallCloudDetailScale = snapshot.SmallCloudDetailScale,
                    smallCloudDensityMultiplier = snapshot.SmallCloudDensityMultiplier,
                    smallCloudDensityBias = snapshot.SmallCloudDensityBias,
                    noiseFrequency = snapshot.NoiseFrequency,
                    noiseScale = snapshot.NoiseScale,
                    noiseThreshold = snapshot.NoiseThreshold,
                    noiseSoftness = snapshot.NoiseSoftness,
                    noiseDensityOffset = snapshot.NoiseDensityOffset
                });
            }

            RenderSettings.skybox = null;

            Camera targetCamera = ResolveCamera();
            if (targetCamera == null)
            {
                return;
            }

            EnsureCameraSupport(targetCamera);
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.black;
        }

        static Camera ResolveCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera;
            }

            return FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
        }

        void ApplyFog(WeatherTransition.WeatherState state, GTA5TimecycleSky.Snapshot snapshot)
        {
            Color fogColor = Color.Lerp(snapshot.SkyPlaneColor, state.FogColor, 0.6f);
            float minimumDensity = Mathf.Lerp(0.0025f, 0.008f, 1f - snapshot.DayNightBalance);
            float density = Mathf.Max(minimumDensity, state.FogDensity);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = density;
        }

        void ApplyDirectionalLight(WeatherTransition.WeatherState state, GTA5TimecycleSky.Snapshot snapshot)
        {
            Light targetLight = ResolveDirectionalLight();
            if (targetLight == null)
            {
                return;
            }

            targetLight.type = LightType.Directional;
            targetLight.color = snapshot.DirectionalLightColor;
            targetLight.intensity = Mathf.Max(0.025f, snapshot.DirectionalLightIntensity);
            targetLight.transform.rotation = Quaternion.LookRotation(-snapshot.DirectionalLightDirection, Vector3.up);
            targetLight.shadows = LightShadows.Soft;
            targetLight.shadowStrength = Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(snapshot.SunFade + 0.2f));
            targetLight.shadowBias = 0.035f;
            targetLight.shadowNormalBias = 0.2f;
        }

        void ApplyPostProcessing(GTA5TimecycleSky.Snapshot snapshot)
        {
            EnsureGlobalVolume();

            if (colorAdjustments != null)
            {
                colorAdjustments.active = true;
                colorAdjustments.postExposure.Override(Mathf.Lerp(-0.15f, 0.18f, snapshot.DayNightBalance));
                colorAdjustments.contrast.Override(Mathf.Lerp(10f, 20f, snapshot.DayNightBalance));
                colorAdjustments.saturation.Override(Mathf.Lerp(5f, 15f, snapshot.DayNightBalance));
                colorAdjustments.colorFilter.Override(Color.Lerp(new Color(0.88f, 0.9f, 1f, 1f), Color.white, snapshot.DayNightBalance));
            }

            if (tonemapping != null)
            {
                tonemapping.active = true;
                tonemapping.mode.Override(TonemappingMode.ACES);
            }

            if (bloom != null)
            {
                bloom.active = true;
                bloom.threshold.Override(0.85f);
                bloom.intensity.Override(Mathf.Lerp(0.15f, 0.45f, snapshot.SunFade + (snapshot.MoonFade * 0.25f)));
                bloom.scatter.Override(0.78f);
                bloom.tint.Override(Color.Lerp(snapshot.MoonColor, snapshot.SunDiscColor, snapshot.SunFade));
            }
        }

        void EnsureCameraSupport(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.allowHDR = true;
            UniversalAdditionalCameraData cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = targetCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            cameraData.renderPostProcessing = true;
        }

        void EnsureGlobalVolume()
        {
            if (globalVolume == null)
            {
                globalVolume = FindFirstObjectByType<Volume>(FindObjectsInactive.Exclude);
                if (globalVolume == null || !globalVolume.isGlobal)
                {
                    GameObject go = new GameObject("GTA5GlobalVolume");
                    go.transform.SetParent(transform, false);
                    globalVolume = go.AddComponent<Volume>();
                    globalVolume.isGlobal = true;
                    globalVolume.priority = 100f;
                }
            }

            if (runtimeVolumeProfile == null)
            {
                runtimeVolumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                runtimeVolumeProfile.name = "GTA5RuntimeVolumeProfile";
                runtimeVolumeProfile.hideFlags = HideFlags.DontSave;
                globalVolume.sharedProfile = runtimeVolumeProfile;
            }

            if (!runtimeVolumeProfile.TryGet(out colorAdjustments))
            {
                colorAdjustments = runtimeVolumeProfile.Add<ColorAdjustments>(true);
            }

            if (!runtimeVolumeProfile.TryGet(out tonemapping))
            {
                tonemapping = runtimeVolumeProfile.Add<Tonemapping>(true);
            }

            if (!runtimeVolumeProfile.TryGet(out bloom))
            {
                bloom = runtimeVolumeProfile.Add<Bloom>(true);
            }
        }

        WeatherTransition.WeatherState GetEffectiveState(WeatherTransition.WeatherState state)
        {
            if (!hasSolarOverride)
            {
                return state;
            }

            return state.WithSolarParameters(overrideSunAzimuth, overrideSunZenith);
        }

        float ResolveTimeOfDay()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = GetComponent<DayNightCycle>();
            }

            if (dayNightCycle != null)
            {
                return dayNightCycle.TimeOfDay;
            }

            if (hasSolarOverride)
            {
                return Mathf.Repeat(overrideSunAzimuth / 360f, 1f) * 24f;
            }

            return 12f;
        }

        // OPT: cache light, only do scene search once
        Light ResolveDirectionalLight()
        {
            if (directionalLight != null)
            {
                return directionalLight;
            }

            directionalLight = RenderSettings.sun;
            if (directionalLight != null && directionalLight.type == LightType.Directional)
            {
                return directionalLight;
            }

            Light[] sceneLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int index = 0; index < sceneLights.Length; index++)
            {
                Light candidate = sceneLights[index];
                if (candidate != null && candidate.type == LightType.Directional)
                {
                    directionalLight = candidate;
                    RenderSettings.sun = directionalLight;
                    return directionalLight;
                }
            }

            return null;
        }
    }
}
