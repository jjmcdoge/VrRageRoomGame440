Shader "Custom/FloorLight_URP"
{
    Properties
    {
        _MainTex ("Floor Texture", 2D) = "white" {}
        _LightTex ("Light Cookie", 2D) = "white" {}
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _Intensity ("Light Intensity", Range(0, 5)) = 1
        _LightPos ("Light Position", Vector) = (0,5,0,0)
        _LightSize ("Light Size", Vector) = (2,0.5,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_LightTex);
            SAMPLER(sampler_LightTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _LightTex_ST;
                float4 _LightColor;
                float4 _LightPos;
                float4 _LightSize;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.worldPos = posInputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 lightDir = IN.worldPos - _LightPos.xyz;

                float width = max(_LightSize.x, 0.0001);
                float height = max(_LightSize.y, 0.0001);

                float2 projUV = float2(
                    lightDir.x / width,
                    lightDir.z / height
                ) + 0.5;

                half4 lightSample = SAMPLE_TEXTURE2D(_LightTex, sampler_LightTex, projUV);
                half4 floorTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                half3 lightContribution = lightSample.rgb * _LightColor.rgb * _Intensity;
                half3 finalColor = floorTex.rgb + lightContribution;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
