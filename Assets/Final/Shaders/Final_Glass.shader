Shader "CG/Final_Glass"
{
    Properties
    {
        _BaseColor ("Glass Color", Color) = (0.15, 0.35, 0.2, 1)
        _Alpha ("Glass Alpha", Range(0, 1)) = 0.25
        _LiquidColor ("Liquid Color", Color) = (0.55, 0.28, 0.07, 1)
        _LiquidAlpha ("Liquid Alpha", Range(0, 1)) = 0.9
        _FillHeight ("Fill Height", Range(0, 0.45)) = 0.25
        _FillBottom ("Fill Bottom", Range(-0.1, 0.45)) = -0.1
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Range(1, 256)) = 128
        [HDR] _RimColor ("Rim Color", Color) = (0.35, 0.45, 0.5, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Alpha;
                float4 _LiquidColor;
                float _LiquidAlpha;
                float _FillHeight;
                float _FillBottom;
                float _Ambient;
                float4 _SpecularColor;
                float _Shininess;
                float4 _RimColor;
                float _RimPower;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  heightOS   : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.heightOS = input.positionOS.y;
                return output;
            }

            float3 BlinnPhong(Light light, float3 N, float3 V, float3 baseColor)
            {
                float3 L = light.direction;
                float3 H = normalize(L + V);
                float NdotL = max(0, dot(N, L));
                float specular = pow(max(0, dot(N, H)), _Shininess) * step(0.0001, NdotL);
                float3 lightColor = light.color * light.distanceAttenuation;
                return baseColor * NdotL * lightColor
                     + _SpecularColor.rgb * specular * lightColor;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(_WorldSpaceCameraPos - input.positionWS);

                float liquid = 1 - smoothstep(_FillHeight - 0.004, _FillHeight + 0.004, input.heightOS);
                liquid *= step(_FillBottom, input.heightOS);
                float hasLiquid = step(0.005, _FillHeight - max(_FillBottom, 0));
                liquid *= hasLiquid;
                float3 baseColor = lerp(_BaseColor.rgb, _LiquidColor.rgb, liquid);
                float baseAlpha = lerp(_Alpha, _LiquidAlpha, liquid);

                float3 color = baseColor * _Ambient;

                color += BlinnPhong(GetMainLight(), N, V, baseColor);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    color += BlinnPhong(light, N, V, baseColor);
                LIGHT_LOOP_END

                float rim = pow(1 - saturate(dot(N, V)), _RimPower);
                color += _RimColor.rgb * rim;

                float surfaceLine = 1 - smoothstep(0, 0.006, abs(input.heightOS - _FillHeight));
                surfaceLine *= hasLiquid;
                color += _LiquidColor.rgb * surfaceLine * 0.8;

                float alpha = saturate(baseAlpha + rim + surfaceLine * 0.3);
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}