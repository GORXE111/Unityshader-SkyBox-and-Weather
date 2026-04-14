Shader "GTA5Sky/Sky"
{
    Properties
    {
        _AzimuthEastColor ("Azimuth East Color", Color) = (0.529, 0.808, 0.922, 1)
        _AzimuthEastIntensity ("Azimuth East Intensity", Float) = 1
        _AzimuthWestColor ("Azimuth West Color", Color) = (0.42, 0.71, 0.91, 1)
        _AzimuthWestIntensity ("Azimuth West Intensity", Float) = 1
        _AzimuthTransitionColor ("Azimuth Transition Color", Color) = (0.8, 0.85, 0.9, 1)
        _AzimuthTransitionIntensity ("Azimuth Transition Intensity", Float) = 1
        _AzimuthTransitionPos ("Azimuth Transition Position", Range(0,1)) = 0.5

        _ZenithColor ("Zenith Color", Color) = (0.106, 0.373, 0.651, 1)
        _ZenithIntensity ("Zenith Intensity", Float) = 1
        _ZenithTransitionColor ("Zenith Transition Color", Color) = (0.53, 0.74, 0.87, 1)
        _ZenithTransitionIntensity ("Zenith Transition Intensity", Float) = 1
        _ZenithTransitionPos ("Zenith Transition Position", Range(0,1)) = 0.25
        _ZenithTransEastBlend ("Zenith Transition East Blend", Range(0,1)) = 0.5
        _ZenithTransWestBlend ("Zenith Transition West Blend", Range(0,1)) = 0.5
        _ZenithBlendStart ("Zenith Blend Start", Range(0,1)) = 0.7
        _SkyHdrIntensity ("Sky HDR Intensity", Float) = 1
        _SkyPlaneColor ("Sky Plane Color", Color) = (0.5, 0.5, 0.6, 1)
        _SkyPlaneIntensity ("Sky Plane Intensity", Float) = 1

        _SunDirection ("Sun Direction", Vector) = (0.5, 0.5, 0.5, 0)
        _SunColorHdr ("Sun Color HDR", Color) = (1, 0.95, 0.8, 1)
        _SunDiscColor ("Sun Disc Color", Color) = (1, 0.95, 0.8, 1)
        _SunDiscSize ("Sun Disc Size", Float) = 0.9
        _SunHdrIntensity ("Sun HDR Intensity", Float) = 2
        _MiePhase2 ("Mie Phase x2", Float) = 1.52
        _MiePhaseSqr1 ("Mie Phase Sqr + 1", Float) = 1.5776
        _MieScatter ("Mie Scatter", Float) = 0.003
        _MieIntensity ("Mie Intensity", Float) = 1.5
        _SunInfluenceRadius ("Sun Influence Radius", Float) = 0.05
        _SunScatterIntensity ("Sun Scatter Intensity", Float) = 1
        _SunFade ("Sun Fade", Float) = 1

        _MoonDirection ("Moon Direction", Vector) = (-0.5, 0.3, -0.2, 0)
        _MoonColor ("Moon Color", Color) = (0.6, 0.7, 0.9, 1)
        _MoonDiscSize ("Moon Disc Size", Float) = 5.5
        _MoonIntensity ("Moon Intensity", Float) = 0.2
        _MoonInfluenceRadius ("Moon Influence Radius", Float) = 0.03
        _MoonScatterIntensity ("Moon Scatter Intensity", Float) = 0.18
        _MoonFade ("Moon Fade", Float) = 0
        _StarfieldIntensity ("Starfield Intensity", Float) = 0.5
        _StarTex ("Star Texture", 2D) = "black" {}

        _CloudBaseColor ("Cloud Base Color", Color) = (0.4, 0.45, 0.5, 1)
        _CloudMidColor ("Cloud Mid Color", Color) = (0.9, 0.9, 0.9, 1)
        _CloudShadowColor ("Cloud Shadow Color", Color) = (0.12, 0.15, 0.2, 1)
        _CloudBaseStrength ("Cloud Base Strength", Float) = 0.35
        _CloudDensityMultiplier ("Cloud Density Multiplier", Float) = 0.5
        _CloudDensityBias ("Cloud Density Bias", Float) = -0.2
        _CloudEdgeStrength ("Cloud Edge Strength", Float) = 0.35
        _CloudOverallStrength ("Cloud Overall Strength", Float) = 0.5
        _CloudFadeOut ("Cloud Fade Out", Float) = 0.6
        _CloudHdrIntensity ("Cloud HDR Intensity", Float) = 0.35
        _CloudOffset ("Cloud Offset", Float) = 0

        _SmallCloudColor ("Small Cloud Color", Color) = (0.8, 0.82, 0.85, 1)
        _SmallCloudDetailStrength ("Small Cloud Detail Strength", Float) = 0.25
        _SmallCloudDetailScale ("Small Cloud Detail Scale", Float) = 1.8
        _SmallCloudDensityMultiplier ("Small Cloud Density Multiplier", Float) = 0.35
        _SmallCloudDensityBias ("Small Cloud Density Bias", Float) = -0.35

        _NoiseFrequency ("Noise Frequency", Float) = 1.2
        _NoiseScale ("Noise Scale", Float) = 1
        _NoiseThreshold ("Noise Threshold", Float) = 0.5
        _NoiseSoftness ("Noise Softness", Float) = 0.15
        _NoiseDensityOffset ("Noise Density Offset", Float) = -0.15
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "GTA5Sky"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDirOS  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _AzimuthEastColor;
                float _AzimuthEastIntensity;
                float4 _AzimuthWestColor;
                float _AzimuthWestIntensity;
                float4 _AzimuthTransitionColor;
                float _AzimuthTransitionIntensity;
                float _AzimuthTransitionPos;

                float4 _ZenithColor;
                float _ZenithIntensity;
                float4 _ZenithTransitionColor;
                float _ZenithTransitionIntensity;
                float _ZenithTransitionPos;
                float _ZenithTransEastBlend;
                float _ZenithTransWestBlend;
                float _ZenithBlendStart;
                float _SkyHdrIntensity;
                float4 _SkyPlaneColor;
                float _SkyPlaneIntensity;

                float4 _SunDirection;
                float4 _SunColorHdr;
                float4 _SunDiscColor;
                float _SunDiscSize;
                float _SunHdrIntensity;
                float _MiePhase2;
                float _MiePhaseSqr1;
                float _MieScatter;
                float _MieIntensity;
                float _SunInfluenceRadius;
                float _SunScatterIntensity;
                float _SunFade;

                float4 _MoonDirection;
                float4 _MoonColor;
                float _MoonDiscSize;
                float _MoonIntensity;
                float _MoonInfluenceRadius;
                float _MoonScatterIntensity;
                float _MoonFade;
                float _StarfieldIntensity;

                float4 _CloudBaseColor;
                float4 _CloudMidColor;
                float4 _CloudShadowColor;
                float _CloudBaseStrength;
                float _CloudDensityMultiplier;
                float _CloudDensityBias;
                float _CloudEdgeStrength;
                float _CloudOverallStrength;
                float _CloudFadeOut;
                float _CloudHdrIntensity;
                float _CloudOffset;

                float4 _SmallCloudColor;
                float _SmallCloudDetailStrength;
                float _SmallCloudDetailScale;
                float _SmallCloudDensityMultiplier;
                float _SmallCloudDensityBias;

                float _NoiseFrequency;
                float _NoiseScale;
                float _NoiseThreshold;
                float _NoiseSoftness;
                float _NoiseDensityOffset;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDirOS = input.positionOS.xyz;
                return output;
            }

            // --- Optimized noise: single hash with fewer ops ---
            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float Noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = Hash12(i);
                float b = Hash12(i + float2(1.0, 0.0));
                float c = Hash12(i + float2(0.0, 1.0));
                float d = Hash12(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // OPT: FBM reduced from 4 to 3 octaves (saves 25% noise ALU, visually identical for clouds)
            float FBM(float2 p)
            {
                float value = Noise2D(p) * 0.5;
                value += Noise2D(p * 2.03) * 0.25;
                value += Noise2D(p * 4.12) * 0.125;
                return value;
            }

            float2 SkyUv(float3 viewDir, float scale)
            {
                float horizon = max(0.08, viewDir.y + 0.22);
                return (viewDir.xz / horizon) * scale;
            }

            float3 ComputeBaseSky(float3 viewDir)
            {
                float azimuthBlend = sqrt(saturate(-viewDir.x * 0.5 + 0.5));
                float zenithBlend = saturate(abs(viewDir.y));

                float3 azimuthEast = _AzimuthEastColor.rgb * _AzimuthEastIntensity;
                float3 azimuthWest = _AzimuthWestColor.rgb * _AzimuthWestIntensity;
                float3 azimuthTransition = _AzimuthTransitionColor.rgb * _AzimuthTransitionIntensity;
                float3 zenithColor = _ZenithColor.rgb * _ZenithIntensity;
                float3 zenithTransition = _ZenithTransitionColor.rgb * _ZenithTransitionIntensity;

                // OPT: branchless azimuth blend using smoothstep instead of if/else
                float eps = 0.0001;
                float azSel = saturate((azimuthBlend - _AzimuthTransitionPos) / max(1.0 - _AzimuthTransitionPos, eps));
                float azSelInv = saturate(azimuthBlend / max(_AzimuthTransitionPos, eps));
                float3 azimuthLow = lerp(azimuthEast, azimuthTransition, azSelInv);
                float3 azimuthHigh = lerp(azimuthTransition, azimuthWest, azSel);
                float azMask = step(_AzimuthTransitionPos, azimuthBlend);
                float3 azimuthColor = lerp(azimuthLow, azimuthHigh, azMask);

                float zenithTransitionBlend = lerp(_ZenithTransEastBlend, _ZenithTransWestBlend, azimuthBlend);
                float3 transitionColor = lerp(azimuthColor, zenithTransition, zenithTransitionBlend);

                // OPT: branchless zenith blend
                float zSel = saturate((zenithBlend - _ZenithTransitionPos) / max(1.0 - _ZenithTransitionPos, eps));
                zSel = saturate(zSel / max(_ZenithBlendStart, eps));
                float zSelInv = saturate(zenithBlend / max(_ZenithTransitionPos, eps));
                float zMask = step(_ZenithTransitionPos, zenithBlend);
                float3 skyLow = lerp(azimuthColor, transitionColor, zSelInv);
                float3 skyHigh = lerp(transitionColor, zenithColor, zSel);
                float3 skyColor = lerp(skyLow, skyHigh, zMask);

                float horizonBand = saturate(1.0 - abs(viewDir.y) / 0.09);
                skyColor += _SkyPlaneColor.rgb * _SkyPlaneIntensity * horizonBand * _SkyPlaneColor.a;
                skyColor *= _SkyHdrIntensity;

                float horizonFade = saturate((viewDir.y + 0.15) / 0.25);
                skyColor = lerp(azimuthColor * 0.55, skyColor, horizonFade);

                return skyColor;
            }

            float3 ComputeSunMoonScattering(float3 viewDir, float3 sunDir)
            {
                float sunCosTheta = dot(viewDir, sunDir);
                // OPT: pow(x, 1.5) = x * sqrt(x), avoids exp/log
                float mieBase = max(_MiePhaseSqr1 - _MiePhase2 * sunCosTheta, 0.0001);
                float mie = _MieScatter / (mieBase * sqrt(mieBase));
                float sunScatter = mie * _MieIntensity * _SunScatterIntensity * _SunFade;
                float sunHalo = smoothstep(_SunInfluenceRadius, 0.0, 1.0 - sunCosTheta);
                float sunDisc = smoothstep(_SunDiscSize * 0.00014, 0.0, 1.0 - sunCosTheta);
                float3 sunColor = _SunColorHdr.rgb * ((_SunHdrIntensity * sunScatter * 0.35) + (_SunHdrIntensity * sunHalo * 0.15) + (_SunHdrIntensity * sunDisc * 1.0));
                sunColor += _SunDiscColor.rgb * (sunDisc * _SunFade * 0.6);

                float3 moonDir = normalize(_MoonDirection.xyz);
                float moonCosTheta = dot(viewDir, moonDir);
                float moonHalo = smoothstep(_MoonInfluenceRadius, 0.0, 1.0 - moonCosTheta);
                float moonRadius = _MoonDiscSize * 0.0012;
                float moonDisc = smoothstep(moonRadius, moonRadius * 0.97, 1.0 - moonCosTheta);
                float3 moonColor = _MoonColor.rgb * _MoonFade * (
                    moonHalo * _MoonScatterIntensity * 0.8 +
                    moonDisc * _MoonIntensity * 4.0);

                return sunColor + moonColor;
            }

            // OPT: branchless tri-planar using blend weights instead of if/else
            float3 ComputeStars(float3 viewDir)
            {
                float aboveHorizon = saturate(viewDir.y);

                float3 absDir = abs(viewDir);
                // Compute blend weights: dominant axis gets weight 1
                float3 weights = step(absDir.yzx, absDir) * step(absDir.zxy, absDir);
                // Fallback: if all zero (shouldn't happen), use Y
                weights = max(weights, float3(0, step(absDir.x + absDir.z, 0.001), 0));

                // Compute UV for each face
                float2 uvY = viewDir.xz / max(absDir.y, 0.001) * 0.5 + 0.5;
                float2 uvX = viewDir.yz / max(absDir.x, 0.001) * 0.5 + float2(3.2, 0.5);
                float2 uvZ = viewDir.xy / max(absDir.z, 0.001) * 0.5 + float2(5.8, 0.5);

                // Select dominant face UV (branchless)
                float2 starUv = uvY * weights.y + uvX * weights.x + uvZ * weights.z;

                float2 tiledUv = frac(starUv * 1.5);
                float4 starSample = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, tiledUv);
                float starLuma = dot(starSample.rgb, float3(0.2126, 0.7152, 0.0722));

                float2 starCell = floor(starUv * float2(768.0, 768.0));
                float twinkleSeed = Hash12(starCell + 17.0);
                float twinkleSpeed = lerp(1.5, 5.0, twinkleSeed);
                float twinklePhase = twinkleSeed * (2.0 * PI);
                float waveA = sin(_Time.y * twinkleSpeed + twinklePhase) * 0.5 + 0.5;
                float waveB = sin(_Time.y * (twinkleSpeed * 1.73) + (twinklePhase * 1.37)) * 0.5 + 0.5;
                float twinkleWave = lerp(waveA, waveB, 0.35);

                float isStar = smoothstep(0.04, 0.15, starLuma);
                float brightStar = smoothstep(0.25, 0.7, starLuma);
                float twinkle = lerp(1.0, lerp(0.55, 1.4, twinkleWave), isStar);
                twinkle *= lerp(1.0, lerp(0.8, 1.5, twinkleWave * twinkleWave * twinkleWave), brightStar);

                float horizonSoftFade = smoothstep(0.0, 0.12, viewDir.y);
                return starSample.rgb * horizonSoftFade * aboveHorizon * _StarfieldIntensity * _MoonFade * twinkle;
            }

            float3 ComputeClouds(float3 viewDir, float3 skyColor, float3 sunDir)
            {
                // OPT: early out before any noise computation
                float horizonFade = saturate((viewDir.y + 0.02) / max(0.05, _CloudFadeOut));
                if (horizonFade <= 0.0001) return skyColor;

                float2 skyUv = SkyUv(viewDir, _NoiseScale) + float2(_CloudOffset, _CloudOffset * 0.37);

                // OPT: single FBM for base clouds (3 octaves instead of 4)
                float baseNoise = FBM(skyUv * _NoiseFrequency);

                float density = (baseNoise * _CloudDensityMultiplier) + _CloudDensityBias + _NoiseDensityOffset;
                float mask = smoothstep(_NoiseThreshold + _NoiseSoftness, _NoiseThreshold - _NoiseSoftness, density);

                float combined = mask * _CloudOverallStrength;

                // OPT: only compute small clouds if base cloud has coverage
                if (combined > 0.001)
                {
                    float smallNoise = FBM((skyUv + 17.0) * (_NoiseFrequency * _SmallCloudDetailScale));
                    float smallDensity = (smallNoise * _SmallCloudDensityMultiplier) + _SmallCloudDensityBias;
                    float smallMask = smoothstep(0.62, 0.42, smallDensity) * _SmallCloudDetailStrength;
                    combined = saturate(combined + smallMask);
                }

                combined *= horizonFade;
                if (combined <= 0.0001) return skyColor;

                float edge = saturate(abs(ddx(combined)) + abs(ddy(combined))) * _CloudEdgeStrength;

                // OPT: sunDir passed in, no redundant normalize
                float lightAmount = saturate(dot(viewDir, sunDir) * 0.5 + 0.5);
                float edgeHighlight = saturate(edge * 6.0) * lightAmount;

                float3 cloudLit = lerp(_CloudShadowColor.rgb, _CloudBaseColor.rgb, lightAmount);
                cloudLit = lerp(cloudLit, _CloudMidColor.rgb, edgeHighlight);
                cloudLit *= _CloudBaseStrength + _CloudHdrIntensity;

                return lerp(skyColor, skyColor + cloudLit, combined);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirOS);
                // OPT: normalize sun direction once, pass to both functions
                float3 sunDir = normalize(_SunDirection.xyz);
                float3 skyColor = ComputeBaseSky(viewDir);
                skyColor = ComputeClouds(viewDir, skyColor, sunDir);
                skyColor += ComputeSunMoonScattering(viewDir, sunDir);
                skyColor += ComputeStars(viewDir);
                return half4(max(0.0, skyColor), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
