Shader "Custom/VehicleLightSystem" {
    Properties {
        // Main Body
        [Header(Main Body Settings)]
        _BodyColor ("Body Color", Color) = (0.1, 0.1, 0.8, 1)
        _BodyTex ("Body Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.95
        _Smoothness ("Smoothness", Range(0,1)) = 0.85
        
        // Headlights
        [Header(Headlight Settings)]
        [HDR]_HeadlightColor ("Headlight Color", Color) = (1,1,1,1)
        _HeadlightTex ("Light Pattern", 2D) = "white" {}
        _HeadlightIntensity ("Intensity", Range(0, 20)) = 5
        _BeamFalloff ("Beam Falloff", Range(0.1, 10)) = 3
        
        // Glass Components
        [Header(Glass Settings)]
        [HDR]_GlassTint ("Glass Tint", Color) = (0.5, 0.6, 0.8, 0.3)
        _GlassSmoothness ("Glass Smoothness", Range(0,1)) = 0.9
        _FresnelPower ("Fresnel Power", Range(0,5)) = 2.5
        
        // Auxiliary Lights
        [Header(Auxiliary Lights)]
        [HDR]_RunningLightColor ("Running Light Color", Color) = (1,0.5,0,1)
        _RunningLightTex ("Light Pattern", 2D) = "white" {}
        _LightFalloff ("Light Falloff", Range(0.1,5)) = 1
    }

    SubShader {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="Geometry+1"
        }
        LOD 500

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 4.0

        struct Input {
            float2 uv_BodyTex;
            float2 uv_HeadlightTex;
            float2 uv_RunningLightTex;
            float3 viewDir;
            float3 worldPos;
        };

        // Body Properties
        sampler2D _BodyTex;
        float4 _BodyColor;
        half _Metallic;
        half _Smoothness;

        // Headlight Properties
        sampler2D _HeadlightTex;
        float4 _HeadlightColor;
        float _HeadlightIntensity;
        float _BeamFalloff;

        // Glass Properties
        float4 _GlassTint;
        half _GlassSmoothness;
        half _FresnelPower;

        // Auxiliary Light Properties
        sampler2D _RunningLightTex;
        float4 _RunningLightColor;
        float _LightFalloff;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Sample main body texture
            fixed4 bodyTex = tex2D(_BodyTex, IN.uv_BodyTex);
            
            // Base surface properties
            o.Albedo = bodyTex.rgb * _BodyColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            // My Headlight System
            float2 hlUV = IN.uv_HeadlightTex * 2 - 1;
            float beamShape = 1 - saturate(pow(abs(hlUV.x), _BeamFalloff) + pow(abs(hlUV.y), _BeamFalloff));
            float4 headlightPattern = tex2D(_HeadlightTex, IN.uv_HeadlightTex);
            
            // Projection effect
            float3 viewProj = 1 - saturate(dot(o.Normal, IN.viewDir));
            float headlightEmission = beamShape * headlightPattern.a * _HeadlightIntensity * viewProj;
            
            // The Auxiliary Light System with calculations
            float2 auxUV = IN.uv_RunningLightTex * 2 - 1;
            float auxShape = 1 - saturate(pow(abs(auxUV.x), _LightFalloff) + pow(abs(auxUV.y), _LightFalloff));
            float4 runningLightPattern = tex2D(_RunningLightTex, IN.uv_RunningLightTex);
            float auxEmission = auxShape * runningLightPattern.a * _RunningLightColor.a;

            // Glass Components with calcualtions
            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _FresnelPower);
            float glassEffect = fresnel * _GlassTint.a;

            // Final Composition
            o.Emission = 
                (_HeadlightColor * headlightEmission) + 
                (_RunningLightColor * auxEmission) + 
                (_GlassTint * glassEffect);

            // Glass Transparency
            o.Alpha = bodyTex.a;
            o.Alpha = lerp(1, glassEffect, _GlassTint.a);
            
            // Glass-specific properties
            o.Smoothness = lerp(_Smoothness, _GlassSmoothness, glassEffect);
            o.Metallic = lerp(_Metallic, 0, glassEffect);
        }

        // Custom Lighting Function for Headlights
        half4 LightingStandard_Special(SurfaceOutputStandard s, half3 viewDir, UnityGI gi) {
            half4 c = LightingStandard(s, viewDir, gi);
            c.rgb += s.Emission;
            return c;
        }

        inline void LightingStandard_Special_GI (
            SurfaceOutputStandard s,
            UnityGIInput data,
            inout UnityGI gi
        ) {
            LightingStandard_GI(s, data, gi);
        }
        
        ENDCG
    }
    
    FallBack "VertexLit"
    CustomEditor "VehicleLightShaderGUI"
}