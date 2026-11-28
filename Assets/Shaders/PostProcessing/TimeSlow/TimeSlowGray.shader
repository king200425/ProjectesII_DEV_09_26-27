Shader "Hidden/TimeSlowGray"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayAmount ("Gray Amount", Range(0, 1)) = 1
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 2
        _Softness ("Softness", Float) = 0.05
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _GrayAmount;
            float4 _Center;
            float _Radius;
            float _Softness;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Convert color to grayscale
                float gray = dot(
                    col.rgb,
                    float3(0.299, 0.587, 0.114)
                );

                // Distance from player position
                float2 offset = i.uv - _Center.xy;
                offset.x *= _ScreenParams.x / _ScreenParams.y;

                float distanceToCenter = length(offset);

                // Circular expansion mask
                float mask = 1.0 - smoothstep(
                    _Radius - _Softness,
                    _Radius,
                    distanceToCenter
                );

                col.rgb = lerp(
                    col.rgb,
                    gray.xxx,
                    mask * _GrayAmount
                );

                return col;
            }

            ENDCG
        }
    }

    Fallback Off
}
