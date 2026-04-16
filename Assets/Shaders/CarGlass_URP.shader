Shader "Custom/TintedCarGlass_URP"
{
    Properties
    {
        [HDR]_TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _SpecularIntensity("Specular Intensity", Range(0, 1)) = 0.5
        _Shininess("Shininess", Range(0.1, 100)) = 50
        _FresnelPower("Fresnel Power", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half _SpecularIntensity;
                half _Shininess;
                half _FresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 viewDirWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                OUT.fogCoord = ComputeFogFactor(positionInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);

                // Base tint and transparency
                half3 albedo = _TintColor.rgb;
                half alpha = saturate(_TintColor.a);

                // Fresnel edge glow
                half fresnel = pow(1.0h - saturate(dot(viewDirWS, normalWS)), max(_FresnelPower, 0.0001h));
                half3 emission = albedo * fresnel * 0.5h;

                // Convert legacy "shininess" into a narrower highlight control.
                // Higher values produce tighter highlights, like the Built-in shader.
                half specPower = max(_Shininess, 0.1h);
                half specularStrength = saturate(_SpecularIntensity);

                // Simple ambient term to keep tint visible.
                half3 color = albedo * 0.2h + emission;

                // Main light
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = saturate(dot(normalWS, lightDir));
                half3 halfDir = normalize(lightDir + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half spec = pow(NdotH, specPower) * specularStrength;

                color += albedo * mainLight.color * NdotL;
                color += mainLight.color * spec;

                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS);
                    half3 addLightDir = normalize(light.direction);
                    half addNdotL = saturate(dot(normalWS, addLightDir));
                    half3 addHalfDir = normalize(addLightDir + viewDirWS);
                    half addNdotH = saturate(dot(normalWS, addHalfDir));
                    half addSpec = pow(addNdotH, specPower) * specularStrength;

                    color += albedo * light.color * addNdotL * light.distanceAttenuation * light.shadowAttenuation;
                    color += light.color * addSpec * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                color = MixFog(color, IN.fogCoord);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
