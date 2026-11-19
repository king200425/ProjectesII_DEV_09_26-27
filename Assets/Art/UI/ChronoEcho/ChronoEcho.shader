Shader "UI/Chrono Echo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Graphic Texture", 2D) = "white" {}
        _Mode ("0 Screen, 1 Echo", Float) = 0
        _Ignition ("Clock Ignition", Range(0,1)) = 0
        _Glitch ("Echo Interference", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            struct AppData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            sampler2D _MainTex;
            float _Mode, _Ignition, _Glitch;

            Varyings Vert(AppData input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            float Hash(float n) { return frac(sin(n * 127.1) * 43758.5453); }
            float Ring(float distance, float radius, float thickness)
            {
                return exp(-abs(distance - radius) / thickness);
            }
            float4 Frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                if (_Mode > .5)
                {
                    float band = floor(input.uv.y * 28.0);
                    float tick = floor(time * 22.0);
                    float tear = (Hash(band + tick * 33.0) - .5) * _Glitch * .15;
                    float2 uv = input.uv + float2(tear, 0);
                    float4 ghost = tex2D(_MainTex, uv);
                    ghost.r = tex2D(_MainTex, uv + float2(.018 * _Glitch, 0)).r;
                    ghost.b = tex2D(_MainTex, uv - float2(.024 * _Glitch, 0)).b;
                    ghost.a *= .83 + .17 * step(.3, frac(input.uv.y * 82.0));
                    return ghost * input.color;
                }
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * float2(1816.0, 1050.0);
                float2 relative = pixel - float2(908.0, 248.0);
                float radius = length(relative);
                float glow = exp(-radius / 164.0) * (.045 + .15 * _Ignition);
                float pulse = Ring(radius, 155.0 + sin(time * 1.6) * 3.0, 9.0) * .10;
                float wavePhase = frac(time * .72);
                float wave = Ring(radius, 165.0 + wavePhase * 410.0, 4.0) * (1.0 - wavePhase) * _Ignition * .38;
                float scan = step(1.0, fmod(pixel.y, 3.0)) * .08;
                float grid = step(fmod(pixel.x, 32.0), .6) * step(fmod(pixel.y, 32.0), .6) * .15;
                float energy = glow + pulse + wave;
                float3 color = float3(.08, .91, 1.0) * saturate(energy * 2.6 + grid);
                float opacity = saturate(scan + energy + grid);
                #ifndef UNITY_COLORSPACE_GAMMA
                    color = GammaToLinearSpace(color);
                #endif
                return float4(color, opacity);
            }
            ENDCG
        }
    }
}
