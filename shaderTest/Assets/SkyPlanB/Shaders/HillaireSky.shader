Shader "SkyPlanB/HillaireSky"
{
    Properties
    {
        _SkyViewLUT ("Sky View LUT", 2D) = "black" {}
        _StarTex ("Star Texture", 2D) = "black" {}
        _SunDirection ("Sun Direction", Vector) = (0, 0.5, 0.5, 0)
        _MoonDirection ("Moon Direction", Vector) = (0, 0.3, -0.5, 0)
        _SunColor ("Sun Color", Color) = (1, 0.98, 0.92, 1)
        _MoonColor ("Moon Color", Color) = (0.6, 0.7, 0.9, 1)
        _SunIntensity ("Sun Intensity", Float) = 50
        _MoonIntensity ("Moon Intensity", Float) = 0.6
        _SunDiscSize ("Sun Disc Size", Float) = 0.9997
        _MoonDiscSize ("Moon Disc Size", Float) = 0.9994
        _Exposure ("Exposure", Float) = 15
        _NightAmbient ("Night Ambient", Float) = 0.012
        _StarIntensity ("Star Intensity", Float) = 0.8
        _SunFade ("Sun Fade", Float) = 1
        _MoonFade ("Moon Fade", Float) = 0
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
            Name "HillaireSky"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SkyViewLUT);
            SAMPLER(sampler_SkyViewLUT);
            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 viewDirOS : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _SunDirection;
                float4 _MoonDirection;
                float4 _SunColor;
                float4 _MoonColor;
                float _SunIntensity;
                float _MoonIntensity;
                float _SunDiscSize;
                float _MoonDiscSize;
                float _Exposure;
                float _NightAmbient;
                float _StarIntensity;
                float _SunFade;
                float _MoonFade;
            CBUFFER_END

            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.viewDirOS = input.positionOS.xyz;
                return o;
            }

            float3 ACESFilm(float3 x)
            {
                float a = 2.51; float b = 0.03; float c = 2.43; float d = 0.59; float e = 0.14;
                return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
            }

            // Hillaire SkyViewLutParamsToUv (exact from source)
            float2 SkyViewParamsToUv(float3 viewDir, float3 sunDir, float viewHeight)
            {
                float R_BOT = 6360.0;
                float Vhorizon = sqrt(max(viewHeight * viewHeight - R_BOT * R_BOT, 0));
                float CosBeta = Vhorizon / viewHeight;
                float Beta = acos(CosBeta);
                float ZenithHorizonAngle = PI - Beta;

                // viewZenithCosAngle = viewDir.y (local up = Y in sky dome)
                float viewZenithCosAngle = viewDir.y;
                bool intersectGround = viewZenithCosAngle < CosBeta;

                float2 uv;
                if (!intersectGround)
                {
                    float coord = acos(viewZenithCosAngle) / ZenithHorizonAngle;
                    coord = 1.0 - coord;
                    coord = sqrt(max(coord, 0)); // non-linear inverse
                    coord = 1.0 - coord;
                    uv.y = coord * 0.5;
                }
                else
                {
                    float coord = (acos(viewZenithCosAngle) - ZenithHorizonAngle) / max(Beta, 0.0001);
                    coord = sqrt(max(coord, 0)); // non-linear inverse
                    uv.y = coord * 0.5 + 0.5;
                }

                // lightViewCosAngle: cos angle between view dir and sun dir projected onto horizon plane
                float3 viewHoriz = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 sunHoriz = normalize(float3(sunDir.x, 0, sunDir.z));
                float lightViewCosAngle = dot(viewHoriz, sunHoriz);

                float coordX = -lightViewCosAngle * 0.5 + 0.5;
                coordX = sqrt(max(coordX, 0)); // non-linear inverse
                uv.x = coordX;

                return uv;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirOS);
                float3 sunDir = normalize(_SunDirection.xyz);
                float viewHeight = 6360.0 + 0.001; // ground level

                // ---- Sky View LUT sampling (Hillaire mapping) ----
                float2 uv = SkyViewParamsToUv(viewDir, sunDir, viewHeight);
                float3 skyColor = SAMPLE_TEXTURE2D(_SkyViewLUT, sampler_SkyViewLUT, uv).rgb;

                // ---- Night ambient ----
                float3 nightColor = float3(0.01, 0.012, 0.03) * _NightAmbient;
                skyColor = max(skyColor, nightColor);

                // ---- Ground: soft fade ----
                if (viewDir.y < 0)
                {
                    float3 horizonColor = SAMPLE_TEXTURE2D(_SkyViewLUT, sampler_SkyViewLUT, float2(uv.x, 0.0)).rgb;
                    horizonColor = max(horizonColor, nightColor);
                    float groundFade = saturate(-viewDir.y / 0.25);
                    skyColor = lerp(horizonColor, horizonColor * 0.4 + nightColor, groundFade);
                }

                // ---- Sun disc ----
                float3 sunDir = normalize(_SunDirection.xyz);
                float sunCos = dot(viewDir, sunDir);
                float sunDisc = smoothstep(_SunDiscSize, _SunDiscSize + 0.0003, sunCos);
                float3 sunContrib = sunDisc * _SunColor.rgb * _SunIntensity * _SunFade;

                // ---- Moon disc + halo ----
                float3 moonDir = normalize(_MoonDirection.xyz);
                float moonCos = dot(viewDir, moonDir);
                float moonDisc = smoothstep(_MoonDiscSize, _MoonDiscSize + 0.0004, moonCos);
                float moonHalo = smoothstep(0.03, 0.0, 1.0 - moonCos) * 0.3;
                float3 moonContrib = (moonDisc * 3.0 + moonHalo) * _MoonColor.rgb * _MoonIntensity * _MoonFade;

                // ---- Stars (only at night) ----
                float3 starColor = 0;
                if (_MoonFade > 0.05 && viewDir.y > 0)
                {
                    float3 absDir = abs(viewDir);
                    float2 starUv;
                    if (absDir.y >= absDir.x && absDir.y >= absDir.z)
                        starUv = viewDir.xz / max(absDir.y, 0.001) * 0.5 + 0.5;
                    else if (absDir.x >= absDir.z)
                        starUv = viewDir.yz / max(absDir.x, 0.001) * 0.5 + float2(3.2, 0.5);
                    else
                        starUv = viewDir.xy / max(absDir.z, 0.001) * 0.5 + float2(5.8, 0.5);

                    float2 tiledUv = frac(starUv * 1.5);
                    float4 starSample = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, tiledUv);
                    float starLuma = dot(starSample.rgb, float3(0.2126, 0.7152, 0.0722));

                    // Twinkle
                    float2 starCell = floor(starUv * 768.0);
                    float seed = Hash12(starCell + 17.0);
                    float speed = lerp(1.5, 5.0, seed);
                    float wave = sin(_Time.y * speed + seed * 6.283) * 0.5 + 0.5;
                    float twinkle = lerp(1.0, lerp(0.5, 1.4, wave), smoothstep(0.04, 0.15, starLuma));

                    float horizFade = smoothstep(0.0, 0.12, viewDir.y);
                    starColor = starSample.rgb * horizFade * _StarIntensity * _MoonFade * twinkle;
                }

                // ---- Combine ----
                float3 color = (skyColor + sunContrib + moonContrib + starColor) * _Exposure;
                color = ACESFilm(color);
                color = pow(max(color, 0), 1.0 / 2.2);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
