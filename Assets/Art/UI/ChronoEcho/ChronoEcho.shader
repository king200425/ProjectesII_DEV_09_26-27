Shader "UI/Chrono Echo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Graphic Texture", 2D) = "white" {}
        _Mode ("0 Screen, 1 Echo", Float) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha
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
                if (_Mode > 1.5)
                {
                    // Perspective rings and continuous light trails pass the viewer.
                    float2 uv = floor(input.uv * float2(640.0, 360.0)) / float2(640.0, 360.0);
                    float entry = smoothstep(0.0, 1.0, _Cover);
                    float2 focus = lerp(_ClockUV.xy, float2(.5, .5), lerp(entry, 1.0, _Reveal));
                    float2 offset = (uv - focus) * float2(_Aspect, 1.0);
                    float radius = length(offset);
                    float angle = atan2(offset.y, offset.x);
                    float turn = angle / 6.2831853 + .5;
                    float3 cyan = float3(.02, .84, 1.0);
                    float3 violet = float3(.65, .29, .98);
                    float3 tint = float3(.004, .008, .026);
                    float envelope = smoothstep(0.0, .26, _Cover);

                    [unroll]
                    for (int i = 0; i < 9; i++)
                    {
                        float depth = frac(i / 9.0 + _Travel * .37);
                        float ringRadius = .025 * pow(60.0, depth);
                        float thickness = .0015 + ringRadius * .004;
                        float ring = Ring(radius, ringRadius, thickness);
                        float dialTurn = turn + _Travel * (.018 + i * .002) * (1.0 - 2.0 * fmod(i, 2.0));
                        float ticks = 1.0 - smoothstep(.025, .07, abs(frac(dialTurn * 60.0) - .5));
                        float major = 1.0 - smoothstep(.03, .09, abs(frac(dialTurn * 12.0) - .5));
                        float tickBand = step(ringRadius, radius) * step(radius, ringRadius * (1.04 + major * .06));
                        float arcs = step(.19, frac(dialTurn * 6.0 + i * .17));
                        float brightness = smoothstep(0.0, .14, depth) * (1.0 - smoothstep(.90, 1.0, depth));
                        float marks = (ring * arcs * .68 + tickBand * (ticks * .65 + major * .4)) * brightness;
                        tint += lerp(cyan, violet, fmod(i, 3.0) * .35) * marks;
                        tint += cyan * Ring(radius, ringRadius, thickness * 5.0) * .07 * brightness;
                    }

                    // Each light has its own depth and speed, with no polygon borders.
                    float sector = floor(turn * 220.0);
                    float seed = Hash(sector * 19.7);
                    float lane = abs(frac(turn * 220.0) - .5);
                    float lightDepth = frac(seed + _Travel * (.5 + Hash(sector + 37.0) * .35));
                    float lightRadius = .022 * pow(70.0, lightDepth);
                    float head = Ring(radius, lightRadius, .002 + lightRadius * .008);
                    float trail = smoothstep(lightRadius * .56, lightRadius, radius) * step(radius, lightRadius);
                    float streak = (1.0 - smoothstep(.035, .13, lane)) * (head + trail * .62);
                    streak *= step(.36, seed) * smoothstep(.015, .08, radius);
                    tint += lerp(cyan, violet, step(.67, seed)) * streak * 1.25;

                    float spiral = .5 + .5 * sin(angle * 3.0 - log(max(radius, .012)) * 5.0 + _Travel * 4.0);
                    tint += lerp(cyan, violet, spiral) * .025 * exp(-radius * 1.9);
                    tint += cyan * exp(-radius * 36.0) * .16;
                    float scan = .93 + .07 * step(1.0, fmod(input.uv.y * 1080.0, 3.0));
                    tint *= scan;
                    #ifndef UNITY_COLORSPACE_GAMMA
                        tint = GammaToLinearSpace(tint);
                    #endif

                    // The actual menu stretches into its clock, leaving temporal echoes.
                    float zoom = 1.0 + pow(entry, 1.5) * 5.0;
                    float2 sceneUV = _ClockUV.xy + (input.uv - focus) / zoom;
                    float split = entry * .009;
                    float3 scene = tex2D(_MainTex, sceneUV).rgb;
                    scene.r = tex2D(_MainTex, sceneUV + float2(split, 0.0)).r;
                    scene.b = tex2D(_MainTex, sceneUV - float2(split, 0.0)).b;
                    float2 echoUV = _ClockUV.xy + (input.uv - focus) / (zoom * 1.1);
                    scene += tex2D(_MainTex, echoUV).rgb * float3(.02, .14, .19) * sin(entry * 3.14159265);
                    #ifndef UNITY_COLORSPACE_GAMMA
                        // ScreenCapture contains the display's gamma-encoded colors.
                        scene = GammaToLinearSpace(scene);
                    #endif
                    float sceneFade = (1.0 - smoothstep(.12, .76, entry)) * (1.0 - _Reveal);
                    float3 color = scene * sceneFade + tint * envelope;

                    // Arrival opens a circular aperture at the far end of the tunnel.
                    float exit = 1.0 - _Cover;
                    float aperture = pow(exit, .9) * 1.55 - .04;
                    float edge = Ring(radius, max(0.0, aperture), .008 + exit * .008) * sin(exit * 3.14159265);
                    color += cyan * edge * _Reveal;
                    float opacity = lerp(smoothstep(0.0, .055, _Cover), smoothstep(aperture - .018, aperture + .018, radius), _Reveal);
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
