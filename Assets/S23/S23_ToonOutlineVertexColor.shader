// S23 과제 5: S20_Toon_Finish + S20_Outline_Finish + S20_VertexColor_Finish 를 하나의 셰이더로 합침
// Pass 2개, 재질 칸 1개
//   ① 툰 + 정점 색 (UniversalForward): 표면 색을 정점 색 × Tint 로 정하고, 확산광을 몇 단계로 끊음
//   ② 외곽선 (SRPDefaultUnlit): normal 방향으로 부풀린 뒷면을 한 가지 색으로
// 참고: S20_ToonOutline_Finish 의 Pass 2개 구조
Shader "CG/S23_ToonOutlineVertexColor"
{
    Properties
    {
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)            // 정점 색에 곱하는 색 (흰색이면 정점 색 그대로)
        _Ambient ("Ambient", Range(0, 1)) = 0.2
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Range(1, 256)) = 32
        _ToonSteps ("Toon Steps", Range(1, 8)) = 3           // 명암을 몇 단계로 끊을지
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        // ① 툰 + 정점 색
        Pass
        {
            Name "ToonVertexColor"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            float _Ambient;
            float4 _SpecularColor;
            float _Shininess;
            float _ToonSteps;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;                   // 정점 색 (VertexColor에서 가져옴)
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 color      : TEXCOORD2;               // 정점 색을 frag로 넘김
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.color      = input.color;             // 삼각형 안에서는 보간됨
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                float3 L = mainLight.direction;                  // 빛이 있는 쪽
                float3 V = normalize(GetCameraPositionWS() - input.positionWS);  // 카메라가 있는 쪽
                float3 R = reflect(-L, N);                       // 빛이 튕겨 나가는 방향

                float diffuse = max(0, dot(N, L));               // 확산광의 밝기
                diffuse = floor(diffuse * _ToonSteps) / _ToonSteps;   // 툰: 몇 단계로 끊음

                float specular = pow(max(0, dot(R, V)), _Shininess);  // 반사광의 밝기

                float3 surfaceColor = input.color.rgb * _BaseColor.rgb;   // 정점 색 × Tint

                float3 color = surfaceColor * (_Ambient + diffuse * mainLight.color)
                             + _SpecularColor.rgb * specular * mainLight.color;
                return float4(color, 1);
            }
            ENDHLSL
        }

        // ② 외곽선
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }    // URP가 UniversalForward와 함께 그려 주는 Pass
            Cull Front                                   // 앞면은 그리지 않고 뒷면만 그림

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 pushed = input.positionOS + input.normalOS * _OutlineWidth;   // normal 방향으로 부풀림
                output.positionCS = TransformObjectToHClip(pushed);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;                    // 외곽선 색 하나
            }
            ENDHLSL
        }
    }
}
