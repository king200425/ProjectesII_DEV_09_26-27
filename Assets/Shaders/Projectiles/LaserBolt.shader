Shader "Effects/Laser Bolt"
{
    Properties
    {
        [HDR] _BoltColor ("Bolt Color", Color) = (1, 0.12, 0.08, 1)
        _Glow ("Glow", Range(0, 2)) = 1
        _Tail ("Tail Opacity", Range(0, 1)) = 0.8
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _LaserLaunch ("Launch Position and Direction", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BoltColor;
                float _Glow;
                float _Tail;
                float4 _LaserLaunch;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 positionWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.positionWS = world.xy;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Quantize in bolt space so the energy bands and the trail share a pixel grid.
                float2 boltPosition = (input.uv - float2(0.75, 0.5)) * float2(1.60, 0.64);
                float2 pixel = floor(boltPosition * 32.0 + 0.5) / 32.0;
                float edge = abs(pixel.y);
                float bodyDistance = length(float2(pixel.x - clamp(pixel.x, -0.19, 0.16), pixel.y));
                float shell = step(bodyDistance, 0.098);
                float inner = step(bodyDistance, 0.065);
                float core = step(bodyDistance, 0.034);

                float tail = smoothstep(-1.08, -0.22, pixel.x) * step(pixel.x, -0.22);
                float tailBody = step(edge, lerp(0.01, 0.075, tail)) * tail;
                float tailCore = step(edge, 0.016) * tail * tail;
                // A pair of staggered streaks gives the short wake some separation at speed.
                float streak = step(abs(pixel.y - 0.09375), 0.015)
                             * smoothstep(-0.86, -0.40, pixel.x) * step(pixel.x, -0.30);
                streak += step(abs(pixel.y + 0.0625), 0.015)
                        * smoothstep(-1.06, -0.62, pixel.x) * step(pixel.x, -0.50);

                float glowDistance = length(float2(boltPosition.x - clamp(boltPosition.x, -0.23, 0.14), boltPosition.y));
                float glow = exp2(-glowDistance * glowDistance * 35.0) * 0.46;
                glow += exp2(-edge * edge * 87.0) * tail * _Tail * 0.25;
                glow *= _Glow * (1.0 - smoothstep(0.24, 0.32, abs(boltPosition.y)));
                glow *= 1.0 - smoothstep(0.29, 0.40, boltPosition.x);

                float3 neon = _BoltColor.rgb;
                float whiteLevel = max(inner * 0.48 + core * 0.52, tailCore * _Tail * 0.46);
                float3 color = lerp(neon, float3(1.0, 1.0, 1.0), saturate(whiteLevel));
                float tailAlpha = max(tailBody, streak * 0.64) * _Tail;
                float alpha = saturate(max(max(shell, tailAlpha), glow));

                // Prevent the wake from drawing behind the muzzle while the bolt is still leaving it.
                float2 forward = _LaserLaunch.zw;
                float travel = dot(input.positionWS - _LaserLaunch.xy, forward);
                float muzzleMask = lerp(1.0, smoothstep(-0.08, 0.04, travel), saturate(dot(forward, forward)));
                return half4(color, alpha * muzzleMask);
            }
            ENDHLSL
        }
    }
}
