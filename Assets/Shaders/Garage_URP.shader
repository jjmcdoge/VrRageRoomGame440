Shader "Custom/NightConcrete_URP"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Roughness ("Roughness", Range(0,1)) = 0.7
        _GlossTint ("Gloss Tint", Color) = (0.5, 0.6, 0.8, 1)
        _ReflectionIntensity ("Reflection Intensity", Range(0,1)) = 0.3
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _SPECULARHIGHLIGHTS_OFF
            #pragma multi_compile_fragment _ _ENVIRONMENTREFLECTIONS_OFF

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                half _Roughness;
                half4 _GlossTint;
                half _ReflectionIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float2 uvBump : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
                half3 tangentWS : TEXCOORD4;
                half3 bitangentWS : TEXCOORD5;
                float fogCoord : TEXCOORD6;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uvBump = TRANSFORM_TEX(IN.uv, _BumpMap);
                OUT.normalWS = NormalizeNormalPerVertex(normInputs.normalWS);
                OUT.tangentWS = NormalizeNormalPerVertex(normInputs.tangentWS);
                OUT.bitangentWS = NormalizeNormalPerVertex(normInputs.bitangentWS);
                OUT.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 albedoSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain);
                half3 albedo = albedoSample.rgb * 0.8h;

                half4 normalTex = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uvBump);
                half3 normalTS = UnpackNormal(normalTex);

                half3x3 tbn = half3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalize(IN.normalWS));
                half3 normalWS = normalize(TransformTangentToWorld(normalTS, tbn));

                half smoothness = saturate((1.0h - _Roughness) * 0.7h);
                half metallic = 0.2h;
                half occlusion = 1.0h;

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = IN.fogCoord;
                inputData.vertexLighting = VertexLighting(IN.positionWS, normalWS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = half4(1,1,1,1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0,0,0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = occlusion;

                half3 reflectionTint = pow(1.0h - smoothness, 2.0h) * _GlossTint.rgb;
                surfaceData.emission = reflectionTint * _ReflectionIntensity;
                surfaceData.alpha = 1.0h;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
