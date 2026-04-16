Shader "Custom/CarPaint_URP"
{
    Properties
    {
        _MainColor ("Base Color", Color) = (0.1, 0.1, 0.8, 1)
        _Metallic ("Metallic", Range(0,1)) = 0.95
        _Smoothness ("Smoothness", Range(0,1)) = 0.85
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,2)) = 0.5
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                half3  normalWS    : TEXCOORD1;
                half3  tangentWS   : TEXCOORD2;
                half3  bitangentWS : TEXCOORD3;
                float2 uvNormal    : TEXCOORD4;
                float2 uvOcclusion : TEXCOORD5;
                half3  viewDirWS   : TEXCOORD6;
                float4 shadowCoord : TEXCOORD7;
                half   fogFactor   : TEXCOORD8;
            };

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_OcclusionMap);
            SAMPLER(sampler_OcclusionMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _MainColor;
                half4 _EmissionColor;
                half _Metallic;
                half _Smoothness;
                half _NormalStrength;
                float4 _NormalMap_ST;
                float4 _OcclusionMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                OUT.tangentWS = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
                OUT.uvNormal = TRANSFORM_TEX(IN.uv, _NormalMap);
                OUT.uvOcclusion = TRANSFORM_TEX(IN.uv, _OcclusionMap);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                OUT.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half3 GetNormalWS(Varyings IN)
            {
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uvNormal), _NormalStrength);
                half3x3 tbn = half3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalize(IN.normalWS));
                half3 normalWS = TransformTangentToWorld(normalTS, tbn);
                return NormalizeNormalPerPixel(normalWS);
            }

            half3 EvaluateLight(half3 albedo, half3 normalWS, half3 viewDirWS, Light light, half metallic, half smoothness)
            {
                half3 lightDir = normalize(light.direction);
                half NdotL = saturate(dot(normalWS, lightDir));
                if (NdotL <= 0.0h)
                    return 0;

                half3 halfDir = SafeNormalize(lightDir + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half shininess = exp2(1.0h + smoothness * 10.0h);
                half spec = pow(NdotH, shininess);

                // Keep the original paint color dominant so the URP result stays close
                // to the Built-in version instead of becoming overly blue/reflective.
                half3 diffuseColor = albedo * (1.0h - metallic * 0.35h);
                half3 specColor = lerp(0.04h.xxx, albedo, metallic * 0.6h);

                return (diffuseColor * NdotL + specColor * spec * saturate(0.25h + smoothness)) * light.color * (light.distanceAttenuation * light.shadowAttenuation);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 packed = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, IN.uvOcclusion);
                half3 normalWS = GetNormalWS(IN);
                half3 viewDirWS = SafeNormalize(IN.viewDirWS);

                half smoothness = saturate(_Smoothness * packed.r);
                half occlusion = saturate(packed.g);
                half3 albedo = _MainColor.rgb;

                // Ambient from spherical harmonics, reduced by the packed occlusion channel.
                half3 ambient = SampleSH(normalWS) * albedo * lerp(1.0h, occlusion, 0.85h);

                Light mainLight = GetMainLight(IN.shadowCoord, IN.positionWS, 1.0h);
                half3 color = ambient + EvaluateLight(albedo, normalWS, viewDirWS, mainLight, _Metallic, smoothness);

                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; ++i)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS, 1.0h);
                    color += EvaluateLight(albedo, normalWS, viewDirWS, light, _Metallic, smoothness);
                }
                #endif

                color += _EmissionColor.rgb;
                color = MixFog(color, IN.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
