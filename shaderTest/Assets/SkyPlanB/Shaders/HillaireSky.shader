Shader "SkyPlanB/HillaireSky"
{
    Properties
    {
        _SkyViewLUT ("Sky View LUT", 2D) = "black" {}
        _SunDirection ("Sun Direction", Vector) = (0, 0.5, 0.5, 0)
        _SunColor ("Sun Color", Color) = (1, 0.98, 0.92, 1)
        _SunIntensity ("Sun Intensity", Float) = 50
        _SunDiscSize ("Sun Disc Size", Float) = 0.9997
        _Exposure ("Exposure", Float) = 15
        _GroundColor ("Ground Color", Color) = (0.05, 0.05, 0.04, 1)
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
                float4 _SunDirection;
                float4 _SunColor;
                float _SunIntensity;
                float _SunDiscSize;
                float _Exposure;
                float4 _GroundColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.viewDirOS = input.positionOS.xyz;
                return o;
            }

            // ACES filmic tone mapping (same as URP)
            float3 ACESFilm(float3 x)
            {
                float a = 2.51;
                float b = 0.03;
                float c = 2.43;
                float d = 0.59;
                float e = 0.14;
                return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirOS);

                // Below horizon: ground color
                if (viewDir.y < -0.01)
                {
                    float fade = saturate((-viewDir.y - 0.01) / 0.15);
                    return half4(_GroundColor.rgb * (1.0 - fade * 0.7), 1);
                }

                // Sky View LUT UV (must match compute shader mapping)
                float latitude = asin(clamp(viewDir.y, -1, 1));
                float longitude = atan2(viewDir.x, viewDir.z);

                // V: non-linear latitude (Hillaire)
                float latParam = sign(latitude) * sqrt(abs(latitude) / (PI * 0.5));
                float v = latParam * 0.5 + 0.5;

                // U: longitude centered (compute uses uv.x-0.5 mapping)
                float u = longitude / (2.0 * PI) + 0.5;

                float3 skyColor = SAMPLE_TEXTURE2D(_SkyViewLUT, sampler_SkyViewLUT, float2(u, v)).rgb;

                // Sun disc (composited on top — LUT can't capture it)
                float3 sunDir = normalize(_SunDirection.xyz);
                float cosAngle = dot(viewDir, sunDir);
                float sunDisc = smoothstep(_SunDiscSize, _SunDiscSize + 0.0003, cosAngle);
                // Sun limb darkening
                float limbDarken = 1.0 - 0.4 * (1.0 - smoothstep(_SunDiscSize + 0.0001, _SunDiscSize + 0.0003, cosAngle));
                float3 sunContrib = sunDisc * limbDarken * _SunColor.rgb * _SunIntensity;

                // Combine and tone map
                float3 color = (skyColor + sunContrib) * _Exposure;
                color = ACESFilm(color);

                // sRGB gamma
                color = pow(max(color, 0), 1.0 / 2.2);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
