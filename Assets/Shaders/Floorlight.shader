Shader "Custom/FloorLight" {
    Properties {
        _MainTex ("Floor Texture", 2D) = "white" {}        // Base floor texture
        _LightTex ("Light Cookie", 2D) = "white" {}        // Projection mask texture, (whats baing projectd on the floor)
        _LightColor ("Light Color", Color) = (1,1,1,1)     // Tint color of the light
        _Intensity ("Light Intensity", Range(0, 5)) = 1    // Brightness multiplier
        _LightPos ("Light Position", Vector) = (0,5,0,0)   // World-space light position (X,Y,Z)
        _LightSize ("Light Size", Vector) = (2,0.5,0,0)    // Light projection area (Width, Height)
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }  // for opaque surfaces
        
        CGPROGRAM
        #pragma surface surf Standard   // Standard lighting model
        #pragma target 3.0              // Shader model target

        struct Input {
            float2 uv_MainTex;         // UV coordinates for floor texture
            float3 worldPos;           // World position of current pixel
        };

        // Shader properties
        sampler2D _MainTex, _LightTex;
        float4 _LightPos;             // XYZ = position, W unused
        float4 _LightSize;            // X = width, Y = height
        float4 _LightColor;
        float _Intensity;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Calculates the light direction from the surface to the light source
            float3 lightDir = IN.worldPos - _LightPos.xyz;
            
           
            // This converts the worlds position to the light's local space
            // Normalized by light size and offset to 0-1 range
            float2 projUV = float2(
                lightDir.x / _LightSize.x,  // X-axis projection
                lightDir.z / _LightSize.y    // Z-axis projection
            ) + 0.5;  // This centers the projection (0.5 = middle of texture)

            // Sampled light cookie texture
            float4 light = tex2D(_LightTex, projUV);
            
            // Sample floor texture
            float4 floorTex = tex2D(_MainTex, IN.uv_MainTex);
            
            // This calculates the final light contribution:
            // by Multiplying cookie textures by color and intensity
            float3 lightContribution = light.rgb * _LightColor.rgb * _Intensity;
            
            // Combining the floor color with light contribution
            o.Albedo = floorTex.rgb + lightContribution;
            
            // I used the floor texture's alpha channel for smoothness
            o.Smoothness = floorTex.a;
        }
        ENDCG
    }
}