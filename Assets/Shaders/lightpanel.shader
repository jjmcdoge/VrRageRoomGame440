Shader "Custom/LightPanel" {
    Properties {
        _MainTex ("Light Pattern", 2D) = "white" {}      // Texture mask for light details
        _Color ("Light Color", Color) = (1,1,1,1)         // Base color tint
        _Intensity ("Intensity", Range(0, 10)) = 2        // Brightness multiplier
        _Falloff ("Falloff", Range(0.1, 5)) = 1           // Edge softness control
    }
    SubShader {
        Tags { 
            "RenderType"="Opaque"        // Opaque surface type
            "Queue"="Geometry+1"         // Renders after regular geometry
        }
        
        CGPROGRAM
        // Standard surface shader with shadow support
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;    // UV coordinates for light pattern
            float3 worldPos;      // World position (unused but available)
        };

        sampler2D _MainTex;
        float4 _Color;
        float _Intensity;
        float _Falloff;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Creates the centered UV coordinates (-1 to 1 range)
            float2 uv = IN.uv_MainTex * 2 - 1;
            
            // This calculates the edge falloff using exponential decay:
            // it Creates rectangular shape with softened edges
            // and Higher _Falloff = sharper edges
            float edge = 1 - saturate(pow(abs(uv.x), _Falloff) + pow(abs(uv.y), _Falloff));
            
            // Sample texture and combine with edge effect
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            float emission = edge * tex.a * _Intensity;  // Uses texture alpha as mask
            
            // Surface properties
            o.Albedo = _Color.rgb * tex.rgb;   // Base color with texture
            o.Emission = _Color.rgb * emission; // Glowing edges + pattern
            o.Metallic = 0;         // Non-metallic surface
            o.Smoothness = 0.2;      // Slightly rough finish
        }
        ENDCG
    }
}
