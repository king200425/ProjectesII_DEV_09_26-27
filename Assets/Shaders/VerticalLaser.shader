Shader "Traps/Vertical Laser"
{
    Properties
    {
        [HDR] _BeamColor ("Beam Color", Color) = (1, 0.07, 0.22, 1)
        [HDR] _CoreColor ("Core Color", Color) = (1, 0.95, 0.88, 1)
        _BeamLength ("Length", Float) = 4
        _BeamWidth ("Width", Float) = 0.65
        _Glow ("Glow Intensity", Float) = 1
        _Speed ("Animation Speed", Float) = 1
        _Variation ("Beam Variation", Range(0, 1)) = 0.9
        _PixelsPerUnit ("Pixels Per Unit", Range(12, 64)) = 24
        _Lightning ("Lightning Intensity", Range(0, 1)) = 0.85
        _Sparks ("Outlet Spark Density", Range(0, 1)) = 0.8
        [HideInInspector] _ShutdownProgress ("Shutdown Progress", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BeamColor;
                float4 _CoreColor;
                float _BeamLength;
                float _BeamWidth;
                float _Glow;
                float _Speed;
                float _Variation;
                float _PixelsPerUnit;
                float _Lightning;
                float _Sparks;
                float _ShutdownProgress;
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
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Hash(float seed)
            {
                float3 value = frac(seed * float3(0.1031, 0.11369, 0.13787));
                value += dot(value, value.yzx + 19.19);
                return frac((value.x + value.y) * value.z);
            }

            float Noise(float coordinate, float seed)
            {
                float cell = floor(coordinate);
                float blend = frac(coordinate);
                return lerp(Hash(cell + seed), Hash(cell + seed + 1.0), blend * blend * (3.0 - 2.0 * blend));
            }

            float Stroke(float distanceToPath, float radius, float pixel)
            {
                return 1.0 - smoothstep(radius * 0.55, radius + pixel, distanceToPath);
            }

            float2 LightningArc(float x, float y, float time, float2 pattern, float seedOffset, float pixel)
            {
                float grid = (y + pattern.y) / pattern.x;
                float cellSeed = floor(grid) * 13.71 + seedOffset;
                float clock = time * lerp(2.0, 4.5, Hash(cellSeed)) + Hash(cellSeed + 1.0);
                float age = frac(clock);
                float seed = cellSeed + floor(clock) * 57.31;
                float start = lerp(0.02, 0.31, Hash(seed + 2.0));
                float span = lerp(0.29, 0.97 - start, pow(Hash(seed + 3.0), 0.65));
                float alongArc = (frac(grid) - start) / span;
                float lifespan = lerp(0.48, 0.94, Hash(seed + 4.0));
                if (alongArc <= 0.0 || alongArc >= 1.0 || age >= lifespan)
                    return float2(0.0, 0.0);

                float angle = Hash(seed + 5.0) * TWO_PI + age * lerp(-2.8, 2.8, Hash(seed + 6.0));
                float sweep = lerp(0.7, 4.4, Hash(seed + 7.0)) * (Hash(seed + 8.0) < 0.5 ? -1.0 : 1.0);
                float steps = floor(lerp(3.0, 8.0, Hash(seed + 9.0)));
                float warped = pow(alongArc, lerp(0.58, 1.6, Hash(seed + 10.0))) * steps;
                float node = floor(warped);
                float2 endpoints = node + float2(0.0, 1.0);
                float2 xNodes = sin(angle + sweep * endpoints / steps) * lerp(0.38, 0.76, Hash(seed + 11.0));
                xNodes += float2(Hash(seed + node * 2.6 + 13.0), Hash(seed + (node + 1.0) * 2.6 + 13.0)) * 0.44 - 0.22;
                float arcX = lerp(xNodes.x, xNodes.y, frac(warped));
                float slope = clamp((xNodes.y - xNodes.x) * steps / (span * pattern.x), -4.0, 4.0);
                float distanceToArc = abs(x - arcX) * rsqrt(1.0 + slope * slope);
                float strength = smoothstep(0.0, 0.065, age) * (1.0 - smoothstep(lifespan * 0.62, lifespan, age));
                strength *= smoothstep(0.0, 0.12, min(alongArc, 1.0 - alongArc));
                strength *= lerp(0.85, 1.0, Hash(seed + 14.0));
                float stroke = Stroke(distanceToArc, lerp(0.012, 0.031, Hash(seed + 15.0)), pixel);
                float halo = exp2(-distanceToArc * distanceToArc * 105.0);

                // Short forks start on the main path and kick out at different angles.
                float forkStart = lerp(0.14, 0.57, Hash(seed + 16.0));
                float forkLength = lerp(0.13, 0.29, Hash(seed + 17.0));
                float forkProgress = (alongArc - forkStart) / forkLength;
                if (forkProgress > 0.0 && forkProgress < 1.0 && Hash(seed + 18.0) > 0.24)
                {
                    float forkSide = Hash(seed + 19.0) < 0.5 ? -1.0 : 1.0;
                    float forkX = arcX + forkSide * (forkProgress * 0.24 + min(forkProgress, 1.0 - forkProgress) * 0.16);
                    float forkDistance = abs(x - forkX) * rsqrt(1.0 + slope * slope);
                    float taper = 1.0 - smoothstep(0.52, 1.0, forkProgress);
                    stroke = max(stroke, Stroke(forkDistance, 0.014, pixel) * taper * 0.73);
                    halo = max(halo, exp2(-forkDistance * forkDistance * 160.0) * taper * 0.58);
                }
                return float2(stroke, halo) * strength;
            }

            float OutletSparks(float2 position, float time, float pixel)
            {
                float sparks = 0.0;
                // Staggered lifetimes create short jets to both sides of the outlet.
                [loop]
                for (int index = 0; index < 10; index++)
                {
                    float emitterSeed = index * 7.0 + 1.0;
                    float clock = time * lerp(2.1, 4.0, Hash(emitterSeed)) + Hash(emitterSeed + 1.0);
                    float life = frac(clock);
                    float seed = emitterSeed + floor(clock) * 97.3;
                    float side = Hash(seed + 6.0) < 0.5 ? -1.0 : 1.0;
                    float2 velocity = float2(side * lerp(0.92, 1.85, Hash(seed + 2.0)),
                                            lerp(0.16, 1.10, Hash(seed + 3.0)));
                    float2 head = float2(side * 0.22, 0.0) + velocity * life;
                    head.y += 0.38 * life * life;
                    float2 direction = normalize(velocity + float2(0.0, 0.76 * life));
                    float2 delta = position - head;
                    float tailLength = lerp(0.08, 0.31, Hash(seed + 4.0)) * life;
                    delta -= direction * clamp(dot(delta, direction), -tailLength, 0.0);
                    float radius = lerp(0.026, 0.048, Hash(seed + 5.0));
                    float stroke = Stroke(max(abs(delta.x), abs(delta.y)), radius, pixel);
                    float fade = smoothstep(0.0, 0.08, life) * (1.0 - smoothstep(0.32, 1.0, life));
                    sparks = max(sparks, stroke * fade * step((index + 1) / 10.0, _Sparks));
                }
                return sparks;
            }

            float4 ShutdownEffect(float across, float along, float grid, float pixel, float cap)
            {
                float progress = saturate(_ShutdownProgress);
                float height = along / _BeamLength;
                float collapse = smoothstep(0.0, 0.47, progress);
                float breakup = smoothstep(0.36, 0.76, progress);
                float darkness = 1.0 - smoothstep(0.49, 0.99, progress);
                float thin = max(grid * 0.46, lerp(0.46, 0.035, collapse));
                float gap = Noise(along * 9.2 + 0.12, 37.4);
                float continuity = step(breakup * 0.83, gap);
                float jitter = (Noise(along * 15.0, 142.15) - 0.5) * breakup * 0.42;
                float distanceToCore = abs(across - jitter);
                float core = Stroke(distanceToCore, thin, pixel) * continuity;
                float rim = Stroke(distanceToCore, thin + 0.07, pixel) * continuity * 0.8;
                float aura = Stroke(distanceToCore, thin + 0.25, pixel) * continuity * _Glow * 0.35;

                // Two pixel flashes cross the centre line as it snaps.
                float burst = smoothstep(0.18, 0.35, progress) * (1.0 - smoothstep(0.55, 0.75, progress));
                float flashDistance = min(abs(height - 0.09), abs(height - 0.89)) * _BeamLength;
                float crossFlash = Stroke(flashDistance, max(0.014, grid * _BeamWidth * 0.55), grid * _BeamWidth * 0.55);
                crossFlash *= Stroke(abs(across), lerp(0.4, 1.16, breakup), pixel) * burst;

                // A few square fragments travel away from the core before fading out.
                float rowPitch = 0.39;
                float row = floor(along / rowPitch);
                float seed = row * 8.17 + 65.1;
                float side = Hash(seed + 1.0) < 0.5 ? -1.0 : 1.0;
                float drift = saturate((progress - 0.31) * 1.65);
                float sparkX = side * lerp(0.09, lerp(0.53, 1.34, Hash(seed + 2.0)), drift);
                float sparkY = rowPitch * (row + lerp(0.18, 0.67, Hash(seed + 3.0))) + drift * 0.075;
                float2 delta = float2(across - sparkX, (along - sparkY) / _BeamWidth);
                float chip = Stroke(max(abs(delta.x), abs(delta.y)), grid * lerp(0.45, 0.85, Hash(seed + 4.0)), pixel);
                chip *= step(0.31, Hash(seed)) * smoothstep(0.34, 0.57, progress)
                      * (1.0 - smoothstep(lerp(0.67, 0.83, Hash(seed + 5.0)), 1.0, progress));

                float highlight = max(core * 0.98, max(crossFlash, chip));
                float3 color = lerp(_BeamColor.rgb, _CoreColor.rgb, highlight);
                float alpha = saturate(max(max(rim, aura), highlight)) * cap * darkness;
                return float4(color * alpha, alpha);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // Sample on a square grid in local space so the steps stay the same size along the beam.
                float rawAlong = input.uv.x * _BeamLength;
                float rawAcross = (input.uv.y - 0.5) * 4.0;
                float grid = min(rcp(_PixelsPerUnit), _BeamWidth / 5.0);
                float gridAcross = grid / _BeamWidth;
                float along = min((floor(rawAlong / grid) + 0.5) * grid, max(_BeamLength - grid * 0.5, 0.0));
                float across = floor(rawAcross / gridAcross + 0.5) * gridAcross;
                float time = floor(_Time.y * _Speed * 18.0) / 18.0;
                float profile = abs(across);
                float pixel = max(fwidth(rawAcross) * 0.55, gridAcross * 0.42);
                float end = min(along, _BeamLength - along);
                float cap = smoothstep(0.0, min(0.08, _BeamLength * 0.19), end);
                float edgeMask = 1.0 - smoothstep(1.67, 2.0, profile);
                if (_ShutdownProgress >= 0.14)
                    return ShutdownEffect(across, along, gridAcross, pixel, cap);

                // Broad energy surges and narrow streaks travel down the pillar at different speeds.
                float surge = Noise(along * 1.8 - time * 7.2, 23.1);
                float turbulence = Noise(along * 8.0 - time * 19.0, 92.7);
                float peak = smoothstep(0.57, 0.95, surge);
                float edgeFlow = Noise(along * 4.8 - time * 10.6, across < 0.0 ? 51.3 : 113.7);
                float rim = 0.48 + (edgeFlow - 0.59) * 0.12 * _Variation;
                float sheath = 1.0 - smoothstep(rim - pixel, rim + pixel, profile);
                float innerWidth = lerp(0.33, 0.20 + surge * 0.10 + peak * 0.13 + turbulence * 0.04, _Variation);
                innerWidth = min(innerWidth, rim - gridAcross);
                float inner = 1.0 - smoothstep(innerWidth - pixel, innerWidth + pixel, profile);
                float center = (Noise(along * 4.1 - time * 11.9, 163.7) - 0.5) * 0.28 * _Variation;
                float filament = 1.0 - smoothstep(0.032, lerp(0.23, 0.09 + surge * 0.13, _Variation), abs(across - center));
                float streakCenter = center * -0.72 + 0.13;
                float streak = Stroke(abs(across - streakCenter), 0.029, pixel)
                             * smoothstep(0.34, 0.88, Noise(along * 3.8 - time * 18.0, 71.3));
                float coreMix = saturate(inner * (lerp(0.66, 0.23 + surge * 0.38 + turbulence * 0.14, _Variation)
                                          + filament * lerp(0.31, 0.36 + surge * 0.27, _Variation)
                                          + streak * 0.43 * _Variation));
                float aura = exp2(-profile * profile * lerp(3.5, 2.3, surge * _Variation))
                           * _Glow * lerp(0.26, 0.15 + surge * 0.16 + peak * 0.15, _Variation);
                aura = lerp(aura, floor(aura * 16.0 + 0.5) / 16.0, 0.7);
                float outlet = min(0.42, _BeamLength * 0.22);
                float2 nozzle = float2(across, (along - outlet) / _BeamWidth);
                float flare = exp2(-dot(nozzle * float2(1.35, 5.0), nozzle * float2(1.35, 5.0)))
                            * (0.31 + 0.52 * Noise(time * 19.0, 18.4)) * _Sparks;

                float2 arcs = float2(0.0, 0.0);
                if (_Lightning > 0.0)
                {
                    float axial = along / max(_BeamWidth, 0.34);
                    arcs = max(LightningArc(across, axial, time, float2(1.62, 0.21), 43.7, pixel),
                               LightningArc(across, axial, time, float2(2.47, 1.11), 197.3, pixel));
                    arcs *= _Lightning * (1.0 - smoothstep(0.82, 1.14, profile));
                }
                float sparks = 0.0;
                if (_Sparks > 0.0 && nozzle.y > -0.08 && nozzle.y < 1.6)
                    sparks = OutletSparks(nozzle, time, pixel);

                float hotEdge = Stroke(abs(profile - rim + 0.035), 0.025, pixel)
                              * (0.21 + edgeFlow * 0.34) * _Variation;
                float tone = max(coreMix, hotEdge);
                // Discrete color bands give the core a sprite-like highlight and outline.
                tone = floor(saturate(tone) * 5.0 + 0.5) / 5.0;
                float3 beam = lerp(_BeamColor.rgb, _CoreColor.rgb, tone);
                float3 arcColor = lerp(_BeamColor.rgb, _CoreColor.rgb, 0.87);
                beam = lerp(beam, arcColor, saturate(arcs.x * 1.24));
                float sparkLight = max(flare, sparks);
                beam = lerp(beam, _CoreColor.rgb, sparkLight * 0.92);
                float alpha = saturate(max(max(aura, sheath * 0.94), max(arcs.x * 1.14, arcs.y * 0.48) + sparkLight));
                alpha *= cap * edgeMask;
                float4 activeBeam = float4(beam * alpha, alpha);
                if (_ShutdownProgress <= 0.0)
                    return activeBeam;

                return lerp(activeBeam, ShutdownEffect(across, along, gridAcross, pixel, cap),
                            smoothstep(0.0, 0.14, _ShutdownProgress));
            }
            ENDHLSL
        }
    }
}
