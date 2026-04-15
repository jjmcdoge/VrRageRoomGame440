Shader "Custom/CarPaint" {
    Properties {
        // The color of the car paint
        _MainColor ("Base Color", Color) = (0.1, 0.1, 0.8, 1)
        
        // This is for the Metallic property (0 = non-metal, 1 = full metal)
        _Metallic ("Metallic", Range(0,1)) = 0.95
        
        // the Surface smoothness/roughness
        _Smoothness ("Smoothness", Range(0,1)) = 0.85
        
        // Normal map for surface details and make the texture pop
        _NormalMap ("Normal Map", 2D) = "bump" {}
        
        // Intensity and strength of the normal map effect
        _NormalStrength ("Normal Strength", Range(0,2)) = 0.5
        
        // Occlusion map for ambient shading
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        
        // Glowing emission color with HDR support which also enables bloom and glow effects, and the emission color controls the intensity of light being emitted by the material
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader {
        
        Tags { 
            "RenderType"="Opaque"  // Opaque surface type
            "Queue"="Geometry"     // Standard rendering queue
        }
        
        CGPROGRAM
        // This is the Surface shader declaration using Standard lighting model
        #pragma surface surf Standard fullforwardshadows
        // Target shader model 3.5 for feature support
        #pragma target 3.5

        // Input structure for surface function
        struct Input {
            float2 uv_NormalMap;    // UV coordinates for normal map
            float2 uv_OcclusionMap; // UV coordinates for occlusion map
            float3 viewDir;         // World-space view direction
        };

        // Shader properties
        sampler2D _NormalMap, _OcclusionMap;
        half4 _MainColor, _EmissionColor;
        half _Metallic;
        half _Smoothness;
        half _NormalStrength;

        // Surface shader function
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Base color 
            o.Albedo = _MainColor.rgb;
            

            // This samples the normal map texture
            // Unpacks and scales normal vectors
            o.Normal = UnpackScaleNormal(tex2D(_NormalMap, IN.uv_NormalMap), _NormalStrength);
            
            // Metallic workflow
            o.Metallic = _Metallic;
            
            // Smoothness is simulated by occlusion map's red channel
            o.Smoothness = _Smoothness * tex2D(_OcclusionMap, IN.uv_OcclusionMap).r;
            
            // Emission color (HDR enabled) makes things look more realistic
            o.Emission = _EmissionColor.rgb;
            
            // Ambient occlusion from occlusion map's green channel
            o.Occlusion = tex2D(_OcclusionMap, IN.uv_OcclusionMap).g;
        }
        ENDCG
    }
    // Fallback shader for unsupported platforms
    FallBack "Diffuse"
}