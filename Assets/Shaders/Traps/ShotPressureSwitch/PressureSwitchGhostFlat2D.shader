Shader "ProjectesII/PressureSwitchGhostFlat2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Activated ("Green unlocked light", Float) = 0
        _IdleLampColor ("Inactive blue lamp", Color) = (0.12, 0.55, 1, 1)
        [HDR] _MetalTint ("Echo body color", Color) = (0.12, 1.35, 1.55, 1)
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex FlatVertex
            #pragma fragment FlatFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };
            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Activated;
                half4 _IdleLampColor;
                half4 _MetalTint;
            CBUFFER_END
            Varyings FlatVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }
            half4 FlatFragment(Varyings input) : SV_Target
            {
                half4 color = CommonUnlitFragment(input, input.color);
                clip(color.a - 0.5h);
                bool lamp = color.r > 0.25h && color.r > color.g * 1.8h && color.r > color.b * 1.8h;
                if (lamp)
                {
                    if (_Activated > 0.5h)
                        color.rgb = half3(0.3h, 1.0h, 0.08h) * color.r;
                    else
                        color.rgb = _IdleLampColor.rgb * color.r;
                    color.rgb = floor(color.rgb * 8.0h + 0.5h) / 8.0h;
                }
                else
                {
                    half metal = dot(color.rgb, half3(0.2126h, 0.7152h, 0.0722h));
                    metal = floor(metal * 8.0h + 0.5h) / 8.0h;
                    color.rgb = half3(metal, metal, metal) * _MetalTint.rgb;
                }
                color.a = 1.0h;
                return color;
            }
            ENDHLSL
        }
    }
}
