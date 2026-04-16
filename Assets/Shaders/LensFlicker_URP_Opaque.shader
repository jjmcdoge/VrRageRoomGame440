Shader "Custom/LensFlicker_URP_Opaque"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BaseColorOff ("Base Color (Off)", Color) = (0,0,0,1)
        _EmissionColor ("Emission Color (On)", Color) = (1,1,1,1)
        _On ("On/Off", Range(0,1)) = 0
        _FlickerSpeed ("Flicker Speed", Float) = 20
        _FlickerIntensity ("Flicker Intensity", Range(0,1)) = 0.5
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
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColorOff;
                float4 _EmissionColor;
                float _On;
                float _FlickerSpeed;
                float _FlickerIntensity;
            CBUFFER_END

            float FlickerFactor(float2 uv)
            {
                float t = _Time.y * _FlickerSpeed;
                float s = sin(t + uv.x * 12.9898) * 43758.5453;
                s = frac(s) * 2.0 - 1.0;   // pseudo-random in [-1,1]
                return lerp(1.0, s, _FlickerIntensity);
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                half3 albedo = lerp(_BaseColorOff.rgb, tex.rgb, saturate(_On));

                float flick = 1.0;
                if (_On > 0.5)
                    flick = FlickerFactor(i.uv);

                half3 emission = _EmissionColor.rgb * saturate(_On) * flick;

                half3 finalColor = albedo + emission;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
