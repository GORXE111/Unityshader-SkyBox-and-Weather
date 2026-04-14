using UnityEngine;

namespace GTA5Sky
{
    public static class GTA5TimecycleSky
    {
        public const float SunRiseTime = 6f;
        public const float SunSetTime = 20f;
        public const float SunMoonTime = SunSetTime + 1.5f;
        public const float MoonSunTime = SunRiseTime - 1.5f;

        const float SunPathSlopeDegrees = 55f;
        const float SunYawOffsetDegrees = 15f;

        public readonly struct Snapshot
        {
            public readonly Color AzimuthEastColor;
            public readonly float AzimuthEastIntensity;
            public readonly Color AzimuthWestColor;
            public readonly float AzimuthWestIntensity;
            public readonly Color AzimuthTransitionColor;
            public readonly float AzimuthTransitionIntensity;
            public readonly float AzimuthTransitionPosition;

            public readonly Color ZenithColor;
            public readonly float ZenithIntensity;
            public readonly Color ZenithTransitionColor;
            public readonly float ZenithTransitionIntensity;
            public readonly float ZenithTransitionPosition;
            public readonly float ZenithTransitionEastBlend;
            public readonly float ZenithTransitionWestBlend;
            public readonly float ZenithBlendStart;
            public readonly float SkyHdrIntensity;
            public readonly Color SkyPlaneColor;
            public readonly float SkyPlaneIntensity;

            public readonly Vector3 SunDirection;
            public readonly Color SunColor;
            public readonly Color SunDiscColor;
            public readonly float SunDiscSize;
            public readonly float SunHdrIntensity;
            public readonly float MiePhase;
            public readonly float MieScatter;
            public readonly float MieIntensity;
            public readonly float SunInfluenceRadius;
            public readonly float SunScatterIntensity;
            public readonly float SunFade;

            public readonly Vector3 MoonDirection;
            public readonly Color MoonColor;
            public readonly float MoonDiscSize;
            public readonly float MoonIntensity;
            public readonly float MoonInfluenceRadius;
            public readonly float MoonScatterIntensity;
            public readonly float MoonFade;
            public readonly float StarfieldIntensity;

            public readonly Color CloudBaseColor;
            public readonly Color CloudMidColor;
            public readonly Color CloudShadowColor;
            public readonly float CloudBaseStrength;
            public readonly float CloudDensityMultiplier;
            public readonly float CloudDensityBias;
            public readonly float CloudEdgeStrength;
            public readonly float CloudOverallStrength;
            public readonly float CloudFadeOut;
            public readonly float CloudHdrIntensity;
            public readonly float CloudOffset;

            public readonly Color SmallCloudColor;
            public readonly float SmallCloudDetailStrength;
            public readonly float SmallCloudDetailScale;
            public readonly float SmallCloudDensityMultiplier;
            public readonly float SmallCloudDensityBias;

            public readonly float NoiseFrequency;
            public readonly float NoiseScale;
            public readonly float NoiseThreshold;
            public readonly float NoiseSoftness;
            public readonly float NoiseDensityOffset;

            public readonly Vector3 DirectionalLightDirection;
            public readonly float DirectionalLightIntensity;
            public readonly Color DirectionalLightColor;
            public readonly float DayNightBalance;

            public Snapshot(
                Color azimuthEastColor, float azimuthEastIntensity,
                Color azimuthWestColor, float azimuthWestIntensity,
                Color azimuthTransitionColor, float azimuthTransitionIntensity, float azimuthTransitionPosition,
                Color zenithColor, float zenithIntensity,
                Color zenithTransitionColor, float zenithTransitionIntensity,
                float zenithTransitionPosition, float zenithTransitionEastBlend, float zenithTransitionWestBlend,
                float zenithBlendStart, float skyHdrIntensity, Color skyPlaneColor, float skyPlaneIntensity,
                Vector3 sunDirection, Color sunColor, Color sunDiscColor, float sunDiscSize, float sunHdrIntensity,
                float miePhase, float mieScatter, float mieIntensity,
                float sunInfluenceRadius, float sunScatterIntensity, float sunFade,
                Vector3 moonDirection, Color moonColor, float moonDiscSize, float moonIntensity,
                float moonInfluenceRadius, float moonScatterIntensity, float moonFade, float starfieldIntensity,
                Color cloudBaseColor, Color cloudMidColor, Color cloudShadowColor,
                float cloudBaseStrength, float cloudDensityMultiplier, float cloudDensityBias,
                float cloudEdgeStrength, float cloudOverallStrength, float cloudFadeOut,
                float cloudHdrIntensity, float cloudOffset,
                Color smallCloudColor, float smallCloudDetailStrength, float smallCloudDetailScale,
                float smallCloudDensityMultiplier, float smallCloudDensityBias,
                float noiseFrequency, float noiseScale, float noiseThreshold, float noiseSoftness, float noiseDensityOffset,
                Vector3 directionalLightDirection, float directionalLightIntensity, Color directionalLightColor,
                float dayNightBalance)
            {
                AzimuthEastColor = azimuthEastColor;
                AzimuthEastIntensity = azimuthEastIntensity;
                AzimuthWestColor = azimuthWestColor;
                AzimuthWestIntensity = azimuthWestIntensity;
                AzimuthTransitionColor = azimuthTransitionColor;
                AzimuthTransitionIntensity = azimuthTransitionIntensity;
                AzimuthTransitionPosition = azimuthTransitionPosition;
                ZenithColor = zenithColor;
                ZenithIntensity = zenithIntensity;
                ZenithTransitionColor = zenithTransitionColor;
                ZenithTransitionIntensity = zenithTransitionIntensity;
                ZenithTransitionPosition = zenithTransitionPosition;
                ZenithTransitionEastBlend = zenithTransitionEastBlend;
                ZenithTransitionWestBlend = zenithTransitionWestBlend;
                ZenithBlendStart = zenithBlendStart;
                SkyHdrIntensity = skyHdrIntensity;
                SkyPlaneColor = skyPlaneColor;
                SkyPlaneIntensity = skyPlaneIntensity;
                SunDirection = sunDirection;
                SunColor = sunColor;
                SunDiscColor = sunDiscColor;
                SunDiscSize = sunDiscSize;
                SunHdrIntensity = sunHdrIntensity;
                MiePhase = miePhase;
                MieScatter = mieScatter;
                MieIntensity = mieIntensity;
                SunInfluenceRadius = sunInfluenceRadius;
                SunScatterIntensity = sunScatterIntensity;
                SunFade = sunFade;
                MoonDirection = moonDirection;
                MoonColor = moonColor;
                MoonDiscSize = moonDiscSize;
                MoonIntensity = moonIntensity;
                MoonInfluenceRadius = moonInfluenceRadius;
                MoonScatterIntensity = moonScatterIntensity;
                MoonFade = moonFade;
                StarfieldIntensity = starfieldIntensity;
                CloudBaseColor = cloudBaseColor;
                CloudMidColor = cloudMidColor;
                CloudShadowColor = cloudShadowColor;
                CloudBaseStrength = cloudBaseStrength;
                CloudDensityMultiplier = cloudDensityMultiplier;
                CloudDensityBias = cloudDensityBias;
                CloudEdgeStrength = cloudEdgeStrength;
                CloudOverallStrength = cloudOverallStrength;
                CloudFadeOut = cloudFadeOut;
                CloudHdrIntensity = cloudHdrIntensity;
                CloudOffset = cloudOffset;
                SmallCloudColor = smallCloudColor;
                SmallCloudDetailStrength = smallCloudDetailStrength;
                SmallCloudDetailScale = smallCloudDetailScale;
                SmallCloudDensityMultiplier = smallCloudDensityMultiplier;
                SmallCloudDensityBias = smallCloudDensityBias;
                NoiseFrequency = noiseFrequency;
                NoiseScale = noiseScale;
                NoiseThreshold = noiseThreshold;
                NoiseSoftness = noiseSoftness;
                NoiseDensityOffset = noiseDensityOffset;
                DirectionalLightDirection = directionalLightDirection;
                DirectionalLightIntensity = directionalLightIntensity;
                DirectionalLightColor = directionalLightColor;
                DayNightBalance = dayNightBalance;
            }
        }

        public static Snapshot Build(WeatherTransition.WeatherState state, float timeOfDay, float timeSeconds)
        {
            float hours = Mathf.Repeat(timeOfDay, 24f);

            Vector3 sunDir = CalculateSunDirection(hours, SunPathSlopeDegrees, SunYawOffsetDegrees);
            Vector3 moonDir = CalculateMoonDirection(hours, SunPathSlopeDegrees, SunYawOffsetDegrees, timeSeconds);
            CalculateDirectionalLight(hours, sunDir, moonDir, out Vector3 directionalDir, out float directionalIntensity, out float sunFade, out float moonFade);

            float clearFactor = Mathf.Clamp01(1f - Mathf.Max(state.RainIntensity * 0.8f, state.FogDensity * 3.5f));
            float cloudiness = Mathf.Clamp01((1f - clearFactor) + (1f - Mathf.Clamp01(state.DirectionalLightIntensity)));
            float night = Smooth01(Mathf.Clamp01(-sunDir.y / 0.28f));
            float twilight = Smooth01(Mathf.Clamp01(1f - (Mathf.Abs(sunDir.y) / 0.48f))) * Mathf.Lerp(0.65f, 1f, sunFade);
            float dawnWeight = twilight * Mathf.Clamp01(sunDir.x);
            float duskWeight = twilight * Mathf.Clamp01(-sunDir.x);
            float dayNightBalance = Mathf.Clamp01(sunFade - (moonFade * 0.5f));

            // sunHeight: 0 at horizon, 1 at zenith — drives daytime color variation
            float sunHeight = Mathf.Clamp01(sunDir.y / 0.82f);
            float sunHeightSq = sunHeight * sunHeight;
            // morningAfternoon: 1 at sunrise/sunset side, 0 at noon — uses sun's X to detect low angles
            float morningAfternoon = Mathf.Clamp01(1f - sunHeight) * sunFade * (1f - twilight);

            Color warmHorizon = new Color(1f, 0.55f, 0.28f);
            Color roseGlow = new Color(0.95f, 0.38f, 0.52f);
            Color lavenderMist = new Color(0.55f, 0.35f, 0.72f);
            Color deepTwilight = new Color(0.18f, 0.12f, 0.42f);
            Color nightZenith = new Color(0.008f, 0.012f, 0.06f);
            Color nightHorizon = new Color(0.025f, 0.03f, 0.09f);

            // Daytime palette — varies with sun height
            Color morningZenith = new Color(0.22f, 0.45f, 0.78f);     // soft blue, slightly muted
            Color noonZenith = new Color(0.12f, 0.38f, 0.72f);        // deeper, more saturated blue
            Color afternoonZenith = new Color(0.18f, 0.42f, 0.68f);   // slightly warmer blue
            Color morningHorizon = new Color(0.75f, 0.72f, 0.62f);    // warm haze
            Color noonHorizon = new Color(0.55f, 0.68f, 0.78f);       // cool clean horizon
            Color afternoonHorizon = new Color(0.72f, 0.65f, 0.55f);  // golden warm horizon

            Color azimuthEast = MultiplyRgb(state.AzimuthEastColor, state.AzimuthEastIntensity);
            Color azimuthWest = MultiplyRgb(state.AzimuthWestColor, state.AzimuthWestIntensity);
            Color azimuthTransition = state.AzimuthTransitionColor;
            Color zenith = MultiplyRgb(state.ZenithColor, state.ZenithIntensity);
            Color zenithTransition = state.ZenithTransitionColor;

            // ---- Daytime sun-height driven variation ----
            // Morning (sun low, sunDir.x > 0) vs afternoon (sun low, sunDir.x < 0) vs noon (sun high)
            float morningWeight = Mathf.Clamp01(sunDir.x) * morningAfternoon;
            float afternoonWeight = Mathf.Clamp01(-sunDir.x) * morningAfternoon;
            float noonWeight = sunHeightSq * sunFade;

            // Zenith: shifts from morning soft blue -> noon deep blue -> afternoon warm blue
            Color dayZenith = Color.Lerp(zenith, morningZenith, morningWeight * 0.5f);
            dayZenith = Color.Lerp(dayZenith, noonZenith, noonWeight * 0.45f);
            dayZenith = Color.Lerp(dayZenith, afternoonZenith, afternoonWeight * 0.5f);
            zenith = Color.Lerp(zenith, dayZenith, sunFade * (1f - twilight) * (1f - night));

            // Horizon: morning warm -> noon cool -> afternoon golden
            Color dayHorizonEast = Color.Lerp(azimuthEast, morningHorizon, morningWeight * 0.35f);
            dayHorizonEast = Color.Lerp(dayHorizonEast, noonHorizon, noonWeight * 0.3f);
            azimuthEast = Color.Lerp(azimuthEast, dayHorizonEast, sunFade * (1f - twilight) * (1f - night));

            Color dayHorizonWest = Color.Lerp(azimuthWest, afternoonHorizon, afternoonWeight * 0.35f);
            dayHorizonWest = Color.Lerp(dayHorizonWest, noonHorizon, noonWeight * 0.3f);
            azimuthWest = Color.Lerp(azimuthWest, dayHorizonWest, sunFade * (1f - twilight) * (1f - night));

            // Transition zone subtly shifts with time of day
            azimuthTransition = Color.Lerp(azimuthTransition,
                Color.Lerp(azimuthEast, azimuthWest, 0.5f), sunFade * 0.2f);

            // Zenith transition warms slightly in morning/afternoon
            zenithTransition = Color.Lerp(zenithTransition,
                Color.Lerp(zenithTransition, Color.Lerp(morningHorizon, afternoonHorizon, 0.5f), 0.15f),
                morningAfternoon * 0.4f);

            // ---- Twilight blending (dawn/dusk) ----
            azimuthEast = Color.Lerp(azimuthEast, warmHorizon, dawnWeight);
            azimuthEast = Color.Lerp(azimuthEast, roseGlow, dawnWeight * 0.4f);
            azimuthWest = Color.Lerp(azimuthWest, warmHorizon, duskWeight);
            azimuthWest = Color.Lerp(azimuthWest, roseGlow, duskWeight * 0.5f);
            azimuthTransition = Color.Lerp(azimuthTransition, Color.Lerp(roseGlow, lavenderMist, 0.45f), twilight * 0.85f);
            zenithTransition = Color.Lerp(zenithTransition, lavenderMist, twilight * 0.7f);
            zenith = Color.Lerp(zenith, deepTwilight, twilight * 0.5f);

            // ---- Night blending ----
            azimuthEast = Color.Lerp(azimuthEast, nightHorizon, night);
            azimuthWest = Color.Lerp(azimuthWest, nightHorizon, night);
            azimuthTransition = Color.Lerp(azimuthTransition, Color.Lerp(nightHorizon, deepTwilight, 0.5f), night);
            zenithTransition = Color.Lerp(zenithTransition, Color.Lerp(nightHorizon, nightZenith, 0.5f), night);
            zenith = Color.Lerp(zenith, nightZenith, night);

            // ---- HDR intensity also varies with sun height ----
            float skyHdr = Mathf.Lerp(0.22f, 1.05f, Mathf.Clamp01(sunFade + (twilight * 0.3f)));
            skyHdr *= Mathf.Lerp(0.85f, 1.12f, sunHeightSq);  // brighter at noon, slightly dimmer morning/evening
            skyHdr = Mathf.Lerp(skyHdr, 0.7f, cloudiness * 0.4f);

            // ---- Horizon plane color ----
            Color skyPlaneColor = Color.Lerp(Color.Lerp(azimuthEast, azimuthWest, 0.5f), state.FogColor, 0.45f);
            skyPlaneColor = Color.Lerp(skyPlaneColor, Color.Lerp(warmHorizon, roseGlow, 0.4f), twilight * 0.45f);
            // Morning/afternoon warm haze on horizon
            skyPlaneColor = Color.Lerp(skyPlaneColor,
                Color.Lerp(morningHorizon, afternoonHorizon, Mathf.Clamp01(-sunDir.x * 0.5f + 0.5f)),
                morningAfternoon * 0.25f);
            float skyPlaneIntensity = Mathf.Lerp(0.1f, 1.1f, Mathf.Clamp01(sunFade + twilight * 0.4f));

            // ---- Sun color also shifts with height ----
            Color sunColor = Color.Lerp(state.DirectionalLightColor, warmHorizon, twilight * 0.55f);
            sunColor = Color.Lerp(sunColor, roseGlow, twilight * 0.25f);
            // Low sun = warmer, high sun = whiter
            sunColor = Color.Lerp(sunColor, Color.Lerp(warmHorizon, Color.white, sunHeight), morningAfternoon * 0.2f);
            sunColor = MultiplyRgb(sunColor, Mathf.Lerp(0.4f, 1.3f, Mathf.Clamp01(sunFade + twilight * 0.3f)));
            Color sunDiscColor = Color.Lerp(state.SunDiscColor, sunColor, 0.65f);
            float sunHdr = Mathf.Lerp(0.6f, 1.8f, Mathf.Clamp01(sunFade + twilight * 0.2f));

            Color moonColor = Color.Lerp(new Color(0.58f, 0.67f, 0.9f), state.DirectionalLightColor, 0.15f);
            float moonIntensity = moonFade * Mathf.Lerp(0.25f, 0.85f, clearFactor);
            float starIntensity = moonFade * clearFactor * Mathf.Lerp(0.15f, 1.35f, night);

            float weatherCloudBoost = Mathf.Clamp01((state.RainIntensity * 0.75f) + (state.FogDensity * 2.5f));
            Color cloudBase = Color.Lerp(Color.Lerp(zenithTransition, state.FogColor, 0.32f), warmHorizon, twilight * 0.15f);
            Color cloudMid = Color.Lerp(Color.white, sunDiscColor, twilight * 0.35f);
            cloudMid = Color.Lerp(cloudMid, Color.Lerp(zenithTransition, Color.white, 0.35f), cloudiness * 0.55f);
            Color cloudShadow = Color.Lerp(zenith, state.FogColor * 0.65f, 0.35f);

            float cloudBaseStrength = Mathf.Lerp(0.12f, 0.75f, weatherCloudBoost);
            float cloudDensityMultiplier = Mathf.Lerp(0.25f, 1.18f, cloudiness);
            float cloudDensityBias = Mathf.Lerp(-0.42f, 0.28f, cloudiness);
            float cloudEdgeStrength = Mathf.Lerp(0.18f, 0.85f, clearFactor);
            float cloudOverallStrength = Mathf.Lerp(0.12f, 1f, cloudiness);
            float cloudFadeOut = Mathf.Lerp(0.22f, 0.82f, cloudiness);
            float cloudHdr = Mathf.Lerp(0.2f, 0.75f, Mathf.Clamp01(clearFactor + twilight * 0.3f));
            float cloudOffset = Mathf.Repeat(timeSeconds * Mathf.Lerp(0.0015f, 0.012f, state.WindSpeed), 1000f);

            Color smallCloudColor = Color.Lerp(cloudMid, azimuthTransition, 0.25f);
            float smallCloudDetailStrength = Mathf.Lerp(0.1f, 0.55f, clearFactor);
            float smallCloudDetailScale = Mathf.Lerp(1.2f, 2.4f, clearFactor);
            float smallCloudDensityMultiplier = Mathf.Lerp(0.2f, 0.8f, clearFactor);
            float smallCloudDensityBias = Mathf.Lerp(-0.5f, -0.12f, clearFactor);

            float noiseFrequency = Mathf.Lerp(0.9f, 1.7f, clearFactor);
            float noiseScale = Mathf.Lerp(0.8f, 1.4f, clearFactor);
            float noiseThreshold = Mathf.Lerp(0.56f, 0.46f, cloudiness);
            float noiseSoftness = Mathf.Lerp(0.22f, 0.1f, clearFactor);
            float noiseDensityOffset = Mathf.Lerp(-0.28f, 0.12f, cloudiness);

            Color directionalLightColor = Color.Lerp(state.DirectionalLightColor, sunDiscColor, twilight * 0.3f);
            directionalLightColor = Color.Lerp(directionalLightColor, moonColor, moonFade * 0.35f);
            directionalLightColor = MultiplyRgb(directionalLightColor, Mathf.Lerp(0.35f, 1f, directionalIntensity));

            return new Snapshot(
                azimuthEast, 1f,
                azimuthWest, 1f,
                azimuthTransition, 1f,
                Mathf.Clamp01(state.AzimuthTransitionPosition),
                zenith, 1f,
                zenithTransition, 1f,
                Mathf.Clamp(state.ZenithTransitionPosition, state.AzimuthTransitionPosition + 0.05f, 0.95f),
                Mathf.Lerp(0.42f, 0.68f, dawnWeight),
                Mathf.Lerp(0.42f, 0.68f, duskWeight),
                0.72f,
                skyHdr,
                skyPlaneColor,
                skyPlaneIntensity,
                sunDir,
                sunColor,
                sunDiscColor,
                0.12f,
                sunHdr,
                Mathf.Clamp(state.MiePhase, 0.05f, 0.95f),
                Mathf.Max(0.0008f, state.MieScatter),
                Mathf.Max(0.15f, state.MieIntensityMult),
                0.006f,
                Mathf.Lerp(0.25f, 1.05f, Mathf.Clamp01(state.MieIntensityMult)),
                sunFade,
                moonDir,
                moonColor,
                5.4f,
                moonIntensity,
                0.025f,
                0.35f,
                moonFade,
                starIntensity,
                cloudBase,
                cloudMid,
                cloudShadow,
                cloudBaseStrength,
                cloudDensityMultiplier,
                cloudDensityBias,
                cloudEdgeStrength,
                cloudOverallStrength,
                cloudFadeOut,
                cloudHdr,
                cloudOffset,
                smallCloudColor,
                smallCloudDetailStrength,
                smallCloudDetailScale,
                smallCloudDensityMultiplier,
                smallCloudDensityBias,
                noiseFrequency,
                noiseScale,
                noiseThreshold,
                noiseSoftness,
                noiseDensityOffset,
                directionalDir,
                directionalIntensity * state.DirectionalLightIntensity,
                directionalLightColor,
                dayNightBalance);
        }

        static void CalculateDirectionalLight(float hours, Vector3 sunDir, Vector3 moonDir, out Vector3 directionalLightDirection, out float directionalIntensity, out float sunFade, out float moonFade)
        {
            sunFade = 1f;
            moonFade = 0f;

            if (hours > SunSetTime)
            {
                sunFade = 1f - Smooth01(Mathf.Clamp01((hours - SunMoonTime) * 2f));
                moonFade = Smooth01(Mathf.Clamp01((hours - SunMoonTime - 0.25f) * 2f));
            }
            else if (hours < SunRiseTime)
            {
                sunFade = Smooth01(Mathf.Clamp01((hours - MoonSunTime - 0.25f) * 2f));
                moonFade = 1f - Smooth01(Mathf.Clamp01((hours - MoonSunTime) * 2f));
            }

            moonFade *= Mathf.Clamp01(moonDir.y / 0.1f);

            float totalFade = (sunFade * 100f) + moonFade;
            if (totalFade > Mathf.Epsilon)
            {
                float sunWeight = (sunFade * 100f) / totalFade;
                float moonWeight = moonFade / totalFade;
                directionalLightDirection = ((sunDir * sunWeight) + (moonDir * moonWeight)).normalized;
            }
            else
            {
                directionalLightDirection = sunDir;
            }

            directionalLightDirection.y = Mathf.Max(0.18f, directionalLightDirection.y);
            directionalLightDirection.Normalize();
            directionalIntensity = Mathf.Max(sunFade, moonFade);
        }

        static Vector3 CalculateSunDirection(float hours, float slopeDegrees, float yawDegrees)
        {
            float slopeRadians = slopeDegrees * Mathf.Deg2Rad;
            float yawRadians = yawDegrees * Mathf.Deg2Rad;
            float angle = CalculateSolarAngle(hours);
            Vector3 direction = new Vector3(
                Mathf.Sin(angle),
                -Mathf.Cos(angle) * Mathf.Sin(slopeRadians),
                -Mathf.Cos(angle) * Mathf.Cos(slopeRadians));

            return Quaternion.AngleAxis(yawRadians * Mathf.Rad2Deg, Vector3.up) * direction.normalized;
        }

        static Vector3 CalculateMoonDirection(float hours, float slopeDegrees, float yawDegrees, float timeSeconds)
        {
            float moonAngle = CalculateSolarAngle(hours) + Mathf.PI;
            float wobble = Mathf.Sin((timeSeconds * 0.03f) + 1.7f) * 0.05f;
            float slopeRadians = (slopeDegrees - 6f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(
                Mathf.Sin(moonAngle),
                (-Mathf.Cos(moonAngle) * Mathf.Sin(slopeRadians)) + wobble,
                -Mathf.Cos(moonAngle) * Mathf.Cos(slopeRadians));

            return Quaternion.AngleAxis((yawDegrees - 28f), Vector3.up) * direction.normalized;
        }

        static float CalculateSolarAngle(float hours)
        {
            if (hours < SunRiseTime)
            {
                return 0.5f * Mathf.PI * (hours / SunRiseTime);
            }

            if (hours < SunSetTime)
            {
                float timeSinceSunrise = hours - SunRiseTime;
                return (0.5f * Mathf.PI) + (Mathf.PI * (timeSinceSunrise / (SunSetTime - SunRiseTime)));
            }

            float timeSinceSunset = hours - SunSetTime;
            return (1.5f * Mathf.PI) + (0.5f * Mathf.PI * (timeSinceSunset / (24f - SunSetTime)));
        }

        static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - (2f * t));
        }

        static Color MultiplyRgb(Color color, float multiplier)
        {
            float scale = Mathf.Max(0f, multiplier);
            return new Color(color.r * scale, color.g * scale, color.b * scale, 1f);
        }
    }
}
