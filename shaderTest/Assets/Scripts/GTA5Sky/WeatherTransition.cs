using UnityEngine;

namespace GTA5Sky
{
    public sealed class WeatherTransition
    {
        public readonly struct WeatherState
        {
            public WeatherState(
                Color skyTopColor,
                Color skyBottomColor,
                float sunAltitude,
                float turbidity,
                float zenithLuminance,
                Color azimuthEastColor,
                float azimuthEastIntensity,
                Color azimuthWestColor,
                float azimuthWestIntensity,
                Color azimuthTransitionColor,
                float azimuthTransitionPosition,
                Color zenithColor,
                float zenithIntensity,
                Color zenithTransitionColor,
                float zenithTransitionPosition,
                float miePhase,
                float mieScatter,
                float mieIntensityMult,
                float sunAzimuth,
                float sunZenith,
                Color sunDiscColor,
                float sunDiscSize,
                Color fogColor,
                float fogDensity,
                Color ambientLight,
                Color directionalLightColor,
                float directionalLightIntensity,
                float rainIntensity,
                float windSpeed,
                float lightningFrequency)
            {
                SkyTopColor = skyTopColor;
                SkyBottomColor = skyBottomColor;
                SunAltitude = sunAltitude;
                Turbidity = turbidity;
                ZenithLuminance = zenithLuminance;
                AzimuthEastColor = azimuthEastColor;
                AzimuthEastIntensity = azimuthEastIntensity;
                AzimuthWestColor = azimuthWestColor;
                AzimuthWestIntensity = azimuthWestIntensity;
                AzimuthTransitionColor = azimuthTransitionColor;
                AzimuthTransitionPosition = azimuthTransitionPosition;
                ZenithColor = zenithColor;
                ZenithIntensity = zenithIntensity;
                ZenithTransitionColor = zenithTransitionColor;
                ZenithTransitionPosition = zenithTransitionPosition;
                MiePhase = miePhase;
                MieScatter = mieScatter;
                MieIntensityMult = mieIntensityMult;
                SunAzimuth = sunAzimuth;
                SunZenith = sunZenith;
                SunDiscColor = sunDiscColor;
                SunDiscSize = sunDiscSize;
                FogColor = fogColor;
                FogDensity = fogDensity;
                AmbientLight = ambientLight;
                DirectionalLightColor = directionalLightColor;
                DirectionalLightIntensity = directionalLightIntensity;
                RainIntensity = rainIntensity;
                WindSpeed = windSpeed;
                LightningFrequency = lightningFrequency;
            }

            public Color SkyTopColor { get; }
            public Color SkyBottomColor { get; }
            public float SunAltitude { get; }
            public float Turbidity { get; }
            public float ZenithLuminance { get; }
            public Color AzimuthEastColor { get; }
            public float AzimuthEastIntensity { get; }
            public Color AzimuthWestColor { get; }
            public float AzimuthWestIntensity { get; }
            public Color AzimuthTransitionColor { get; }
            public float AzimuthTransitionPosition { get; }
            public Color ZenithColor { get; }
            public float ZenithIntensity { get; }
            public Color ZenithTransitionColor { get; }
            public float ZenithTransitionPosition { get; }
            public float MiePhase { get; }
            public float MieScatter { get; }
            public float MieIntensityMult { get; }
            public float SunAzimuth { get; }
            public float SunZenith { get; }
            public Color SunDiscColor { get; }
            public float SunDiscSize { get; }
            public Color FogColor { get; }
            public float FogDensity { get; }
            public Color AmbientLight { get; }
            public Color DirectionalLightColor { get; }
            public float DirectionalLightIntensity { get; }
            public float RainIntensity { get; }
            public float WindSpeed { get; }
            public float LightningFrequency { get; }

            public static WeatherState FromProfile(WeatherSettings.WeatherProfile profile)
            {
                return new WeatherState(
                    profile.skyTopColor,
                    profile.skyBottomColor,
                    profile.sunAltitude,
                    profile.turbidity,
                    profile.zenithLuminance,
                    profile.azimuthEastColor,
                    profile.azimuthEastIntensity,
                    profile.azimuthWestColor,
                    profile.azimuthWestIntensity,
                    profile.azimuthTransitionColor,
                    profile.azimuthTransitionPosition,
                    profile.zenithColor,
                    profile.zenithIntensity,
                    profile.zenithTransitionColor,
                    profile.zenithTransitionPosition,
                    profile.miePhase,
                    profile.mieScatter,
                    profile.mieIntensityMult,
                    profile.sunAzimuth,
                    profile.sunZenith,
                    profile.sunDiscColor,
                    profile.sunDiscSize,
                    profile.fogColor,
                    profile.fogDensity,
                    profile.ambientLight,
                    profile.directionalLightColor,
                    profile.directionalLightIntensity,
                    profile.rainIntensity,
                    profile.windSpeed,
                    profile.lightningFrequency);
            }

            public static WeatherState Lerp(WeatherState from, WeatherState to, float t)
            {
                float progress = Mathf.Clamp01(t);
                return new WeatherState(
                    Color.Lerp(from.SkyTopColor, to.SkyTopColor, progress),
                    Color.Lerp(from.SkyBottomColor, to.SkyBottomColor, progress),
                    Mathf.Lerp(from.SunAltitude, to.SunAltitude, progress),
                    Mathf.Lerp(from.Turbidity, to.Turbidity, progress),
                    Mathf.Lerp(from.ZenithLuminance, to.ZenithLuminance, progress),
                    Color.Lerp(from.AzimuthEastColor, to.AzimuthEastColor, progress),
                    Mathf.Lerp(from.AzimuthEastIntensity, to.AzimuthEastIntensity, progress),
                    Color.Lerp(from.AzimuthWestColor, to.AzimuthWestColor, progress),
                    Mathf.Lerp(from.AzimuthWestIntensity, to.AzimuthWestIntensity, progress),
                    Color.Lerp(from.AzimuthTransitionColor, to.AzimuthTransitionColor, progress),
                    Mathf.Lerp(from.AzimuthTransitionPosition, to.AzimuthTransitionPosition, progress),
                    Color.Lerp(from.ZenithColor, to.ZenithColor, progress),
                    Mathf.Lerp(from.ZenithIntensity, to.ZenithIntensity, progress),
                    Color.Lerp(from.ZenithTransitionColor, to.ZenithTransitionColor, progress),
                    Mathf.Lerp(from.ZenithTransitionPosition, to.ZenithTransitionPosition, progress),
                    Mathf.Lerp(from.MiePhase, to.MiePhase, progress),
                    Mathf.Lerp(from.MieScatter, to.MieScatter, progress),
                    Mathf.Lerp(from.MieIntensityMult, to.MieIntensityMult, progress),
                    Mathf.LerpAngle(from.SunAzimuth, to.SunAzimuth, progress),
                    Mathf.Lerp(from.SunZenith, to.SunZenith, progress),
                    Color.Lerp(from.SunDiscColor, to.SunDiscColor, progress),
                    Mathf.Lerp(from.SunDiscSize, to.SunDiscSize, progress),
                    Color.Lerp(from.FogColor, to.FogColor, progress),
                    Mathf.Lerp(from.FogDensity, to.FogDensity, progress),
                    Color.Lerp(from.AmbientLight, to.AmbientLight, progress),
                    Color.Lerp(from.DirectionalLightColor, to.DirectionalLightColor, progress),
                    Mathf.Lerp(from.DirectionalLightIntensity, to.DirectionalLightIntensity, progress),
                    Mathf.Lerp(from.RainIntensity, to.RainIntensity, progress),
                    Mathf.Lerp(from.WindSpeed, to.WindSpeed, progress),
                    Mathf.Lerp(from.LightningFrequency, to.LightningFrequency, progress));
            }

            public WeatherState WithSolarParameters(float sunAzimuth, float sunZenith)
            {
                return new WeatherState(
                    SkyTopColor, SkyBottomColor, sunZenith, Turbidity, ZenithLuminance,
                    AzimuthEastColor, AzimuthEastIntensity, AzimuthWestColor, AzimuthWestIntensity,
                    AzimuthTransitionColor, AzimuthTransitionPosition,
                    ZenithColor, ZenithIntensity, ZenithTransitionColor, ZenithTransitionPosition,
                    MiePhase, MieScatter, MieIntensityMult, sunAzimuth, sunZenith,
                    SunDiscColor, SunDiscSize, FogColor, FogDensity, AmbientLight,
                    DirectionalLightColor, DirectionalLightIntensity,
                    RainIntensity, WindSpeed, LightningFrequency);
            }
        }

        private WeatherState fromState;
        private WeatherState toState;
        private float duration;
        private float elapsedTime;

        public WeatherState CurrentState { get; private set; }
        public bool IsTransitioning => duration > 0f && elapsedTime < duration;

        public void Reset(WeatherState state)
        {
            fromState = state;
            toState = state;
            duration = 0f;
            elapsedTime = 0f;
            CurrentState = state;
        }

        public void Begin(WeatherState currentState, WeatherState nextState, float transitionDuration)
        {
            fromState = currentState;
            toState = nextState;
            duration = Mathf.Max(0f, transitionDuration);
            elapsedTime = 0f;
            CurrentState = duration <= 0f ? nextState : currentState;
        }

        public bool Tick(float deltaTime)
        {
            if (!IsTransitioning)
            {
                return false;
            }

            elapsedTime = Mathf.Min(duration, elapsedTime + Mathf.Max(0f, deltaTime));
            float progress = duration <= 0f ? 1f : elapsedTime / duration;
            CurrentState = WeatherState.Lerp(fromState, toState, progress);
            return true;
        }
    }
}
