using System;
using System.Collections.Generic;
using UnityEngine;

namespace GTA5Sky
{
    [CreateAssetMenu(fileName = "WeatherSettings", menuName = "GTA5Sky/Weather Settings")]
    public sealed class WeatherSettings : ScriptableObject
    {
        [Serializable]
        public sealed class WeatherProfile
        {
            public WeatherType weatherType = WeatherType.Clear;
            public Color skyTopColor = Color.white;
            public Color skyBottomColor = Color.gray;
            [Range(-5f, 90f)] public float sunAltitude = 45f;
            [Min(1f)] public float turbidity = 2f;
            [Min(0.1f)] public float zenithLuminance = 1f;
            public Color azimuthEastColor = Color.white;
            [Min(0f)] public float azimuthEastIntensity = 1f;
            public Color azimuthWestColor = Color.white;
            [Min(0f)] public float azimuthWestIntensity = 1f;
            public Color azimuthTransitionColor = Color.gray;
            [Range(0f, 1f)] public float azimuthTransitionPosition = 0.22f;
            public Color zenithColor = Color.white;
            [Min(0f)] public float zenithIntensity = 1f;
            public Color zenithTransitionColor = Color.white;
            [Range(0f, 1f)] public float zenithTransitionPosition = 0.68f;
            [Range(0f, 1f)] public float miePhase = 0.76f;
            [Min(0f)] public float mieScatter = 0.003f;
            [Min(0f)] public float mieIntensityMult = 1f;
            [Range(0f, 360f)] public float sunAzimuth = 45f;
            [Range(-90f, 90f)] public float sunZenith = 60f;
            public Color sunDiscColor = Color.white;
            [Min(0.1f)] public float sunDiscSize = 4f;
            public Color fogColor = Color.white;
            [Min(0f)] public float fogDensity = 0.01f;
            public Color ambientLight = Color.white;
            public Color directionalLightColor = Color.white;
            [Min(0f)] public float directionalLightIntensity = 1f;
            [Range(0f, 1f)] public float rainIntensity;
            [Range(0f, 1f)] public float windSpeed;
            [Range(0f, 1f)] public float lightningFrequency;
        }

        [SerializeField] private List<WeatherProfile> profiles = new List<WeatherProfile>();

        public IReadOnlyList<WeatherProfile> Profiles => profiles;

        public bool TryGetProfile(WeatherType weatherType, out WeatherProfile profile)
        {
            for (int index = 0; index < profiles.Count; index++)
            {
                WeatherProfile entry = profiles[index];
                if (entry != null && entry.weatherType == weatherType)
                {
                    profile = entry;
                    return true;
                }
            }

            profile = null;
            return false;
        }

        public WeatherProfile GetProfileOrDefault(WeatherType weatherType)
        {
            if (TryGetProfile(weatherType, out WeatherProfile profile))
            {
                return profile;
            }

            WeatherSettings runtimeDefaults = CreateRuntimeDefaults();
            runtimeDefaults.TryGetProfile(weatherType, out WeatherProfile runtimeProfile);
            return runtimeProfile;
        }

        public static WeatherSettings CreateRuntimeDefaults()
        {
            WeatherSettings settings = CreateInstance<WeatherSettings>();
            settings.hideFlags = HideFlags.DontSave;
            WeatherProfile clearProfile = CreateGtaProfile(WeatherType.Clear, Hex(0x4A, 0x90, 0xD9), Hex(0x5B, 0x9E, 0xE3), Hex(0x10, 0x45, 0x8A), 0.76f, 0.003f, 1.25f, 45f, 60f, Hex(0xC8, 0xD8, 0xE8), 0.012f, Hex(0xFF, 0xFA, 0xE0), Hex(0xFF, 0xF0, 0xC7), 1f, 0f, 0.15f, 0f);
            clearProfile.azimuthTransitionColor = Color.Lerp(clearProfile.azimuthWestColor, clearProfile.azimuthEastColor, 0.5f);
            clearProfile.zenithTransitionColor = Color.Lerp(clearProfile.azimuthTransitionColor, clearProfile.zenithColor, 0.72f);
            clearProfile.azimuthEastIntensity = 0.85f;
            clearProfile.azimuthWestIntensity = 1.18f;
            clearProfile.zenithIntensity = 1.12f;
            settings.profiles = new List<WeatherProfile>
            {
                clearProfile,
                CreateGtaProfile(WeatherType.Overcast, Hex(0x77, 0x88, 0xAA), Hex(0x66, 0x77, 0xAA), Hex(0x44, 0x55, 0x66), 0.5f, 0.001f, 0.45f, 45f, 30f, Hex(0x88, 0x99, 0xAA), 0.025f, Hex(0xC0, 0xC0, 0xC0), Hex(0xC7, 0xD6, 0xE8), 0.65f, 0f, 0.25f, 0f),
                CreateGtaProfile(WeatherType.Rainy, Hex(0x44, 0x55, 0x66), Hex(0x44, 0x55, 0x66), Hex(0x33, 0x44, 0x55), 0.3f, 0.0005f, 0.3f, 45f, 20f, Hex(0x55, 0x66, 0x77), 0.06f, Hex(0x88, 0x88, 0x88), Hex(0xB8, 0xC6, 0xD6), 0.45f, 0.55f, 0.55f, 0f),
                CreateGtaProfile(WeatherType.Thunder, Hex(0x2A, 0x3A, 0x4A), Hex(0x2A, 0x3A, 0x4A), Hex(0x1A, 0x2A, 0x3A), 0.2f, 0.0002f, 0.16f, 40f, 15f, Hex(0x33, 0x44, 0x55), 0.09f, Hex(0x55, 0x55, 0x55), Hex(0x9F, 0xB1, 0xC2), 0.3f, 1f, 0.8f, 1f),
                CreateGtaProfile(WeatherType.Foggy, Hex(0xAA, 0xBB, 0xAA), Hex(0xAA, 0xBB, 0xAA), Hex(0xCC, 0xDD, 0xCC), 0.8f, 0.002f, 0.82f, 35f, 35f, Hex(0xCC, 0xDD, 0xCC), 0.2f, Hex(0xBB, 0xBB, 0xBB), Hex(0xE2, 0xE6, 0xD2), 0.5f, 0f, 0.1f, 0f),
                CreateGtaProfile(WeatherType.Smog, Hex(0x99, 0x88, 0x66), Hex(0x88, 0x77, 0x55), Hex(0x77, 0x66, 0x55), 0.9f, 0.005f, 1.45f, 42f, 30f, Hex(0x99, 0x88, 0x77), 0.14f, Hex(0x99, 0x99, 0x66), Hex(0xD7, 0xC2, 0x9E), 0.42f, 0f, 0.18f, 0f)
            };
            return settings;
        }

        private static WeatherProfile CreateGtaProfile(
            WeatherType weatherType,
            Color azimuthEastColor,
            Color azimuthWestColor,
            Color zenithColor,
            float miePhase,
            float mieScatter,
            float mieIntensityMult,
            float sunAzimuth,
            float sunZenith,
            Color fogColor,
            float fogDensity,
            Color ambientLight,
            Color directionalLightColor,
            float directionalLightIntensity,
            float rainIntensity,
            float windSpeed,
            float lightningFrequency)
        {
            Color horizonColor = Color.Lerp(azimuthWestColor, azimuthEastColor, 0.5f);
            Color azimuthTransitionColor = Color.Lerp(horizonColor, zenithColor, 0.35f);
            Color zenithTransitionColor = Color.Lerp(horizonColor, zenithColor, 0.72f);

            return new WeatherProfile
            {
                weatherType = weatherType,
                skyTopColor = zenithColor,
                skyBottomColor = horizonColor,
                sunAltitude = sunZenith,
                turbidity = Mathf.Lerp(2f, 20f, Mathf.Clamp01(mieScatter / 0.005f)),
                zenithLuminance = Mathf.Max(0.1f, directionalLightIntensity),
                azimuthEastColor = azimuthEastColor,
                azimuthEastIntensity = 1f,
                azimuthWestColor = azimuthWestColor,
                azimuthWestIntensity = 1f,
                azimuthTransitionColor = azimuthTransitionColor,
                azimuthTransitionPosition = 0.24f,
                zenithColor = zenithColor,
                zenithIntensity = 1f,
                zenithTransitionColor = zenithTransitionColor,
                zenithTransitionPosition = 0.68f,
                miePhase = miePhase,
                mieScatter = mieScatter,
                mieIntensityMult = mieIntensityMult,
                sunAzimuth = sunAzimuth,
                sunZenith = sunZenith,
                sunDiscColor = directionalLightColor,
                sunDiscSize = Mathf.Lerp(6.5f, 3.5f, directionalLightIntensity),
                fogColor = fogColor,
                fogDensity = fogDensity,
                ambientLight = ambientLight,
                directionalLightColor = directionalLightColor,
                directionalLightIntensity = directionalLightIntensity,
                rainIntensity = rainIntensity,
                windSpeed = windSpeed,
                lightningFrequency = lightningFrequency
            };
        }

        private static Color Hex(byte red, byte green, byte blue)
        {
            return new Color32(red, green, blue, 0xFF);
        }
    }
}
