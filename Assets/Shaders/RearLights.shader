Shader "Custom/MetallicHeadlightSmooth" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.8
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpStrength ("Normal Strength", Range(0,5)) = 1
        _EmissionColor1 ("Red Emission", Color) = (1,0,0,1)
        _EmissionColor2 ("Orange Emission", Color) = (1,0.5,0,1)
        _EmissionIntensity ("Emission Intensity", Range(0,10)) = 3
        _ColorSpeed ("Color Speed", Range(0.1, 2)) = 0.8
        _BlendSharpness ("Blend Sharpness", Range(0.1, 1)) = 0.5
        _NoiseInfluence ("Noise Influence", Range(0, 0.5)) = 0.2
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _FresnelPower ("Fresnel Power", Range(0,5)) = 2.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_NoiseTex;
            float3 worldNormal;
            float3 viewDir;
        };

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _NoiseTex;
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        half _BumpStrength;
        fixed4 _EmissionColor1;
        fixed4 _EmissionColor2;
        half _EmissionIntensity;
        float _ColorSpeed;
        float _BlendSharpness;
        float _NoiseInfluence;
        float _ScrollSpeed;
        float _FresnelPower;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Base color and normal mapping
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = mainTex.rgb * _Color.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)) * _BumpStrength;

            // Metallic properties
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            // used texture and scrolling speeds to emulate moving lights
            float2 scrolledUV = IN.uv_NoiseTex;
            scrolledUV.x += _Time.y * _ScrollSpeed;
            scrolledUV.y += _Time.y * _ScrollSpeed * 0.5;
            half noise = tex2D(_NoiseTex, scrolledUV).r;

            // Smoothed color transition calculation
            float transitionTime = _Time.y * _ColorSpeed;
            float sineWave = sin(transitionTime * 2.0) * 0.5 + 0.5;
            
            // Double eased interpolation
            float pingPong = smoothstep(0.0, 1.0, sineWave);
            float blendFactor = lerp(
                smoothstep(0.2, 0.8, pingPong),
                smoothstep(0.3, 0.7, pingPong),
                _BlendSharpness
            );

            // Adds organic noise variation
            blendFactor = saturate(blendFactor + (noise * _NoiseInfluence));

            // The Final color blending
            fixed3 emissionColor = lerp(
                _EmissionColor1.rgb,
                _EmissionColor2.rgb,
                blendFactor
            );

            // Fresnel effect with normal mapping
            half fresnel = pow(1.0 - saturate(dot(IN.viewDir, o.Normal)), _FresnelPower);

            // Combined emission
            o.Emission = emissionColor * _EmissionIntensity * noise * fresnel;
        }
        ENDCG
    }
    FallBack "Diffuse"
}