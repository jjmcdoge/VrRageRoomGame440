Shader "Custom/LensFlicker"
{
    Properties
    {
        _MainTex       ("Albedo (RGB)", 2D) = "white" {}
        _BaseColorOff  ("Base Color (Off)", Color) = (0,0,0,1)
        _EmissionColor ("Emission Color (On)", Color) = (1,1,1,1)
        _On            ("On/Off", Range(0,1)) = 0
        _FlickerSpeed  ("Flicker Speed", Float)   = 20
        _FlickerIntensity ("Flicker Intensity", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        fixed4   _BaseColorOff;
        fixed4   _EmissionColor;
        float    _On;
        float    _FlickerSpeed;
        float    _FlickerIntensity;

        struct Input {
            float2 uv_MainTex;
        };

        // I created a simple pseudo-random flicker by using sin(time*speed + uv.x*seed)
        float FlickerFactor(float2 uv)
        {
            // base flicker: sin(time) remapped to [0,1]
            float t = _Time.y * _FlickerSpeed;
            float s = sin(t + uv.x * 12.9898) * 43758.5453;
            s = frac(s) * 2 - 1;           // pseudo-random in [-1,1]
            return lerp(1, s, _FlickerIntensity);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // sample albedo
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

            // blend between black and your texture
            o.Albedo = lerp(_BaseColorOff.rgb, tex.rgb, _On);

            // emission only when On > 0
            float flick = 1;
            if (_On > 0.5)
                flick = FlickerFactor(IN.uv_MainTex);

            o.Emission = _EmissionColor.rgb * _On * flick;

            o.Alpha = tex.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

