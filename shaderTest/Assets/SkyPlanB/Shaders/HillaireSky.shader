Shader "SkyPlanB/HillaireSky"
{
    Properties
    {
        _SkyViewLUT ("Sky View LUT", 2D) = "black" {}
        _SunDirection ("Sun Direction", Vector) = (0, 0.5, 0.5, 0)
        _SunColor ("Sun Color", Color) = (1, 0.98, 0.92, 1)
        _SunIntensity ("Sun Intensity", Float) = 30
        _SunDiscSize ("Sun Disc Size", Float) = 0.9995
        _Exposure ("Exposure", Float) = 8
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
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.viewDirOS = input.positionOS.xyz;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirOS);

                // Convert view direction to Sky View LUT UV (Hillaire non-linear mapping)
                float latitude = asin(clamp(viewDir.y, -1, 1));
                float longitude = atan2(viewDir.x, viewDir.z);

                // Non-linear V: more texels near horizon
                float latParam = sign(latitude) * sqrt(abs(latitude) / (PI * 0.5));
                float v = latParam * 0.5 + 0.5;
                float u = longitude / (2.0 * PI) + 0.5;

                float3 skyColor = SAMPLE_TEXTURE2D(_SkyViewLUT, sampler_SkyViewLUT, float2(u, v)).rgb;

                // Sun disc (composited separately — LUT resolution too low)
                float3 sunDir = normalize(_SunDirection.xyz);
                float cosAngle = dot(viewDir, sunDir);
                float sunDisc = smoothstep(_SunDiscSize, _SunDiscSize + 0.0002, cosAngle);
                float3 sunContrib = sunDisc * _SunColor.rgb * _SunIntensity;

                // Tone mapping (simple exposure + Reinhard)
                float3 color = (skyColor + sunContrib) * _Exposure;
                color = color / (1.0 + color); // Reinhard

                // Gamma
                color = pow(max(color, 0), 1.0 / 2.2);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
