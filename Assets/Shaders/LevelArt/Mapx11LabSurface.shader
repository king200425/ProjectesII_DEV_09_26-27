Shader "LevelArt/SciFiLabUnlit"
{
 Properties { _MainTex("Atlas",2D)="white"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
  Cull Off ZWrite Off
  Pass
  {
   Tags { "LightMode"="SRPDefaultUnlit" }
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS); o.uv=v.uv; return o; }
   half4 frag(Varyings i):SV_Target { return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv); }
   ENDHLSL
  }
 }
}
