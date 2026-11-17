Shader "UI/Aftertime Menu"
{
    Properties
    {
        [PerRendererData] _MainTex ("Graphic Texture", 2D) = "white" {}
        [Toggle] _Overlay ("CRT and Window Effects", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct Vertex
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Overlay;
            static const float2 ArtSize = float2(1851.0, 1082.0);
            static const float3 Cyan = float3(0.0, 0.94, 1.0);
            static const float3 Violet = float3(0.71, 0.42, 0.9);
            static const float Pi = 3.14159265;

            Varyings Vert(Vertex input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                return output;
            }

            float Line(float distance, float halfWidth)
            {
                return 1.0 - smoothstep(halfWidth, halfWidth + 1.25, abs(distance));
            }

            float Glow(float distance, float size)
            {
                return exp(-abs(distance) / size);
            }

            float Shockwave(float2 pixel, float2 center, float time, float delay, float dashed)
            {
                float life = frac((time - delay) / 5.5);
                float progress = 1.0 - pow(1.0 - life, 2.6);
                float2 delta = pixel - center;
                float radius = lerp(34.2, 532.0, progress);
                float edge = length(delta) - radius;
                float angle = atan2(delta.y, delta.x);
                float pattern = lerp(1.0, step(0.30, frac(angle * 34.0)), dashed);
                return (0.32 * Line(edge, 1.4) * pattern + 0.43 * Glow(edge, 13.0 + 9.0 * progress)) * (1.0 - life);
            }

            float3 Backdrop(float2 pixel, float time)
            {
                // Interpolate the two circles of the Stitch canvas radial gradient.
                float2 origin = pixel - ArtSize * float2(0.5, 0.32);
                float offsetY = ArtSize.y * 0.18;
                float growth = ArtSize.x * 0.85 - 80.0;
                float a = growth * growth - offsetY * offsetY;
                float b = 80.0 * growth + origin.y * offsetY;
                float c = 80.0 * 80.0 - dot(origin, origin);
                float gradient = saturate((sqrt(max(0.0, b * b - a * c)) - b) / a);
                float3 color = lerp(float3(43.0, 0.0, 82.0), float3(23.0, 6.0, 48.0), saturate(gradient / 0.35));
                color = lerp(color, float3(13.0, 4.0, 26.0), saturate((gradient - 0.35) / 0.35));
                color = lerp(color, float3(5.0, 2.0, 10.0), saturate((gradient - 0.7) / 0.3)) / 255.0;
                float grid = max(step(fmod(pixel.x, 32.0), 0.9), step(fmod(pixel.y, 16.0), 0.9));
                color += 0.012 * grid;

                float2 center = ArtSize * float2(0.5, 0.26);
                float2 delta = pixel - center;
                float distance = length(delta);
                float angle = atan2(delta.y, delta.x);
                color += Violet * (0.38 * Line(distance - 168.0, 1.65) + 0.32 * Line(distance - 85.0, 0.8));
                color += Cyan * (0.29 * Line(distance - 125.0, 0.85) * step(0.45, frac(angle * 7.0 - time * 0.18)));

                for (int ring = 0; ring < 4; ring++)
                {
                    float radius = frac((time * 40.0 + ring * 81.0) / 320.0) * 320.0 + 40.0;
                    float dashes = ring % 2 == 0 ? 1.0 : step(0.44, frac(angle * radius / 14.0));
                    float3 tint = ring % 2 == 0 ? Cyan : Violet;
                    color += tint * (1.0 - radius / 360.0) * dashes * 0.34 * Line(distance - radius, 1.0);
                }

                float sweep = time * 1.4;
                float sweepDelta = frac((sweep - angle) / (2.0 * Pi) + 0.5) * (2.0 * Pi) - Pi;
                float fan = step(0.0, sweepDelta) * (1.0 - smoothstep(0.0, 0.55, sweepDelta));
                float sweepExtent = 1.0 - smoothstep(233.0, 240.0, distance);
                float leadingEdge = Line(sweepDelta * distance, 0.95) * sweepExtent;
                color += Cyan * (0.85 * leadingEdge + 0.35 * fan * (1.0 - saturate(distance / 240.0)));
                float pulse = 0.5 + 0.5 * sin(time * 6.0);
                color += Cyan * (0.91 * Glow(distance, 6.0 + 2.0 * pulse));
                color += Violet * (0.46 * Glow(distance, 12.0 + 2.5 * pulse));

                float2 waveCenter = ArtSize * float2(0.5, 0.16);
                color += Cyan * Shockwave(pixel, waveCenter, time, 0.0, 0.0);
                color += Violet * Shockwave(pixel, waveCenter, time, 1.8, 1.0);
                color += Cyan * Shockwave(pixel, waveCenter, time, 3.6, 0.7);
                return color;
            }

            float Sand(float2 pixel, float x, float time, float period, float delay, float height, float direction)
            {
                float life = frac((time - delay) / period);
                float y = lerp(-110.0, 420.0, direction > 0.0 ? life : 1.0 - life);
                float fade = smoothstep(0.0, 0.2, life) * (1.0 - smoothstep(0.75, 1.0, life));
                float vertical = 1.0 - smoothstep(0.0, height, abs(pixel.y - y));
                float horizontal = 0.53 * Line(pixel.x - x, 2.5) + 0.34 * Glow(pixel.x - x, 5.5);
                float frame = smoothstep(21.0, 39.0, pixel.y) * (1.0 - smoothstep(293.0, 306.0, pixel.y));
                return vertical * horizontal * fade * frame;
            }

            float4 Overlay(float2 pixel, float time)
            {
                float cyan = 0.0;
                float violet = 0.0;
                if (pixel.y < 306.0 && pixel.x < 255.0)
                {
                    cyan = Sand(pixel, 83.0, time, 2.8, 0.0, 64.0, 1.0) + Sand(pixel, 151.0, time, 3.5, 0.7, 88.0, 1.0)
                        + Sand(pixel, 99.0, time, 3.2, 0.5, 40.0, -1.0);
                    violet = Sand(pixel, 220.0, time, 2.4, 1.4, 56.0, 1.0);
                }
                else if (pixel.y < 306.0 && pixel.x > 1596.0)
                {
                    cyan = Sand(pixel, 1688.0, time, 3.8, 1.1, 88.0, 1.0);
                    violet = Sand(pixel, 1631.0, time, 2.6, 0.4, 64.0, 1.0) + Sand(pixel, 1766.0, time, 2.2, 1.6, 56.0, 1.0)
                        + Sand(pixel, 1751.0, time, 3.4, 0.8, 40.0, -1.0);
                }
                float fadeX = sin(pixel.x / ArtSize.x * Pi);
                cyan += 0.22 * fadeX * Glow(pixel.y - frac(time / 7.0) * ArtSize.y, 2.5);
                violet += 0.14 * fadeX * Glow(pixel.y - frac((time - 3.5) / 9.0) * ArtSize.y, 2.5);
                float scan = step(2.0, fmod(pixel.y, 4.0)) * 0.29;
                float3 light = (Cyan * cyan + Violet * violet) * (1.0 - scan);
                float alpha = saturate(scan + max(cyan, violet) * 0.48);
                #ifndef UNITY_COLORSPACE_GAMMA
                    light = GammaToLinearSpace(light);
                #endif
                return float4(light, alpha);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * ArtSize;
                if (_Overlay > 0.5)
                    return Overlay(pixel, _Time.y + 1.8);

                float3 color = saturate(Backdrop(pixel, _Time.y + 1.8));
                #ifndef UNITY_COLORSPACE_GAMMA
                    color = GammaToLinearSpace(color);
                #endif
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
