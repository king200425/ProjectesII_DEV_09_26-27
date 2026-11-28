Shader "UI/Chrono Echo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Graphic Texture", 2D) = "white" {}
        _Mode ("0 Screen, 1 Echo, 2 Tunnel, 3 Particles", Float) = 0
        [HideInInspector] _SrcBlend ("Source Blend", Float) = 5
        [HideInInspector] _DstBlend ("Destination Blend", Float) = 10
        _Ignition ("Clock Ignition", Range(0,1)) = 0
        _Glitch ("Echo Interference", Range(0,1)) = 0
        _Cover ("Transition Coverage", Range(0,1)) = 0
        _Reveal ("Tunnel Exit", Range(0,1)) = 0
        _Travel ("Tunnel Travel", Float) = 0
        _Aspect ("Viewport Aspect", Float) = 1.7777778
        _ClockUV ("Clock Viewport Center", Vector) = (.5, .764, 0, 0)
        _Charge ("Clock Charge", Range(0,1)) = 0
        _Burst ("Clock Burst", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
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
            float _Mode, _Ignition, _Glitch, _Cover, _Reveal, _Charge, _Burst, _Travel, _Aspect;
            float4 _ClockUV;

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
                if (_Mode > 2.5)
                {
                    float3 temporalTint = input.color.rgb;
                    #ifndef UNITY_COLORSPACE_GAMMA
                        temporalTint = GammaToLinearSpace(temporalTint);
                    #endif
                    return float4(temporalTint, input.color.a);
                }
                if (_Mode > 1.5)
                {
                    // Straight perspective lanes stream past a fixed vanishing point.
                    float2 uv = floor(input.uv * float2(640.0, 360.0)) / float2(640.0, 360.0);
                    float entry = smoothstep(0.0, 1.0, _Cover);
                    float2 focus = lerp(_ClockUV.xy, float2(.5, .5), lerp(entry, 1.0, _Reveal));
                    float2 offset = (uv - focus) * float2(_Aspect, 1.0);
                    float radius = length(offset);
                    float angle = atan2(offset.y, offset.x);
                    float turn = angle / 6.2831853 + .5;
                    float3 cyan = float3(.10, .70, 1.0);
                    float3 violet = float3(.68, .39, 1.0);
                    float3 tint = float3(.008, .018, .055);
                    float envelope = smoothstep(0.0, .22, _Cover);

                    // Soft longitudinal streams establish depth without rotating the view.
                    float sector = floor(turn * 100.0);
                    float seed = Hash(sector * 19.7);
                    float lane = abs(frac(turn * 100.0) - .5);
                    float lightDepth = frac(seed + _Travel * (.43 + Hash(sector + 37.0) * .21));
                    float lightRadius = .018 * exp(lightDepth * 4.6);
                    float trail = smoothstep(lightRadius * .22, lightRadius, radius) * step(radius, lightRadius);
                    float head = Ring(radius, lightRadius, .002 + lightRadius * .003);
                    float beam = (1.0 - smoothstep(.025, .11, lane)) * (trail * .65 + head);
                    beam *= step(.22, seed) * smoothstep(.025, .12, radius);
                    float haze = pow(saturate(1.0 - lane * 2.0), 4.0) * trail * .065;
                    tint += lerp(cyan, violet, step(.72, seed)) * (beam + haze);

                    float broadLane = .5 + .5 * sin(angle * 9.0 + Hash(floor(turn * 9.0)) * 3.0);
                    float streamPulse = .65 + .35 * sin(radius * 5.0 - _Travel * 5.0 + seed * 6.0);
                    float3 streamTint = lerp(cyan, violet, smoothstep(-.25, .45, offset.x));
                    tint += streamTint * pow(broadLane, 12.0) * streamPulse * .11 * smoothstep(.035, .25, radius);
                    tint += cyan * exp(-radius * 5.0) * .13;
                    tint += float3(.76, .94, 1.0) * exp(-radius * 38.0) * .9;
                    float scan = .96 + .04 * step(1.0, fmod(input.uv.y * 1080.0, 3.0));
                    tint *= scan;
                    #ifndef UNITY_COLORSPACE_GAMMA
                        tint = GammaToLinearSpace(tint);
                    #endif

                    // Departure stretches towards the light and leaves brief color echoes.
                    float zoom = 1.0 + pow(entry, 1.7) * 8.0;
                    float2 sceneUV = _ClockUV.xy + (input.uv - focus) / zoom;
                    float split = entry * .007;
                    float3 scene = tex2D(_MainTex, sceneUV).rgb;
                    scene.r = tex2D(_MainTex, sceneUV + float2(split, 0.0)).r;
                    scene.b = tex2D(_MainTex, sceneUV - float2(split, 0.0)).b;
                    float2 echoUV = _ClockUV.xy + (input.uv - focus) / (zoom * 1.15);
                    scene += tex2D(_MainTex, echoUV).rgb * float3(.03, .12, .2) * sin(entry * 3.14159265);
                    #ifndef UNITY_COLORSPACE_GAMMA
                        // ScreenCapture contains the display's gamma-encoded colors.
                        scene = GammaToLinearSpace(scene);
                    #endif
                    float sceneFade = (1.0 - smoothstep(.03, .55, entry)) * (1.0 - _Reveal);
                    float3 color = scene * sceneFade + tint * envelope;

                    // Arrival passes through the front light into the new scene.
                    float exit = 1.0 - _Cover;
                    float daylight = exp(-radius / (.025 + exit * 3.0)) * sin(exit * 3.14159265) * _Reveal;
                    color += float3(.68, .9, 1.0) * daylight * .9;
                    float opacity = lerp(smoothstep(0.0, .055, _Cover), 1.0 - smoothstep(.12, .94, exit), _Reveal);
                    opacity = lerp(opacity, 0.0, step(.999, exit) * _Reveal);
                    return float4(color, opacity);
                }
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
                float charge = pow(_Charge, 2.5) * (1.0 - smoothstep(0.0, .24, _Burst));
                float chargedCore = exp(-radius / 15.0) * charge;
                float burstActive = step(.001, _Burst);
                float blast = Ring(radius, _Burst * 1350.0, 8.0 + _Burst * 12.0) * (1.0 - _Burst) * burstActive;
                float flash = (1.0 - smoothstep(.02, .24, _Burst)) * exp(-radius / 90.0) * burstActive;
                energy += chargedCore * .8 + blast * .85 + flash;
                float3 color = float3(.08, .91, 1.0) * saturate(energy * 2.6 + grid);
                color = lerp(color, float3(.91, 1.0, 1.0), saturate(chargedCore + flash));
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
