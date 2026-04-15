Shader "Custom/TintedCarGlass"
{
    Properties
    {
        // HDR color for glass tint and transparency (alpha channel)
        [HDR]_TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        
        // It managaes the intensity of specular highlights
        _SpecularIntensity("Specular Intensity", Range(0, 1)) = 0.5
        
        // manipulates the size of specular highlights (higher means tighter spots)
        _Shininess("Shininess", Range(0.1, 100)) = 50
        
        // Controls edge highlight strength, which comes from the Fresnel effect
        _FresnelPower("Fresnel Power", Range(0, 5)) = 1
    }

    SubShader
    {
        // Render queue and type setup for transparent materials
        Tags { 
            "Queue"="Transparent"    // This Renders after opaque objects
            "RenderType"="Transparent" // Identifying as transparent type
        }
        LOD 200 // Level of Detail quality

        CGPROGRAM
        // This is the surface shader with Blinn-Phong lighting and alpha transparency
        #pragma surface surf BlinnPhong alpha:fade
        #pragma target 3.0 // Shader model target

        // Input structure containing view direction and surface normal
        struct Input
        {
            float3 viewDir;     // Direction from surface to camera
            float3 worldNormal; // World space normal vector
        };

        // Shader properties
        fixed4 _TintColor;
        half _SpecularIntensity;
        half _Shininess;
        half _FresnelPower;

        // Surface shader function
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Base color and transparency setup
            o.Albedo = _TintColor.rgb; // Diffuse color
            o.Alpha = _TintColor.a;    // Transparency level

            // Specular parameters setup
            o.Specular = _SpecularIntensity; // Specular intensity
            o.Gloss = _Shininess;            // Specular exponent

            // Fresnel effect calculation for edge highlights:
            // - Creates stronger reflections at grazing angles
            // - Uses dot product between view direction and surface normal
            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _FresnelPower);
            
            // Add emissive edge effect (50% intensity of base color)
            o.Emission = _TintColor.rgb * fresnel * 0.5;
        }

        // Here is my custom lighting function for Blinn-Phong model
        half4 LightingBlinnPhong_PrePass(SurfaceOutput s, half4 light)
        {
            // Helps Calculate the specular contribution
            half3 spec = light.a * s.Gloss * s.Specular;
            
            // Combine diffuse and specular lighting
            half4 c;
            c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
            
            // Preserves the transparency with specular influence
            c.a = s.Alpha + Luminance(spec);
            
            return c;
        }
        ENDCG
    }
    // Fallback for unsupported platforms
    FallBack "Transparent/VertexLit"
}
