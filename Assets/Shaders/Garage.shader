Shader "Custom/NightConcrete" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}        // Base color texture
        _BumpMap ("Normal Map", 2D) = "bump" {}           // Surface detail normals
        _Roughness ("Roughness", Range(0,1)) = 0.7        // Surface micro-detail (0 = smooth)
        _GlossTint ("Gloss Tint", Color) = (0.5, 0.6, 0.8, 1)  // Cool-toned night reflections
        _ReflectionIntensity ("Reflection Intensity", Range(0,1)) = 0.3 // Gloss visibility
    }
    SubShader {
        Tags { "RenderType"="Opaque" }  // it makes this Suitable for solid surfaces
        
        CGPROGRAM
        #pragma surface surf Standard    
        #pragma target 3.0               

        struct Input {
            float2 uv_MainTex;          // Albedo UV coordinates
            float2 uv_BumpMap;          // Normal map UV coordinates
            float3 worldRefl;           // World reflection vector
        };

        sampler2D _MainTex, _BumpMap;
        float _Roughness;
        float4 _GlossTint;
        float _ReflectionIntensity;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // This is the base color with night-time darkness adjustment
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * 0.8;  // 20% brightness reduction

            // Normal map processing
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            // Surface characteristics
            o.Smoothness = (1 - _Roughness) * 0.7;  // I converted roughness to smoothness
            o.Metallic = 0.2;  // Minimal metalness (concrete contains some minerals)

            // I wanted to create a wet night looking scene
            // - I created an Inverse relationship between smoothness and reflection
            // - Color tinted cool blue for night affect
            // - and I wanted the intensity of it to be controlled by the artist
            float3 reflection = pow(1 - o.Smoothness, 2) * _GlossTint.rgb;
            o.Emission = reflection * _ReflectionIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"  
}


