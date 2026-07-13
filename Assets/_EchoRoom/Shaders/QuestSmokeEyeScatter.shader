Shader "EchoRoom/Quest Smoke Eye Scatter"
{
 Properties
 {
  _BaseMap("Smoke Flipbook", 2D) = "white" {}
  _Tint("Tint", Color) = (0.42,0.43,0.46,0.68)
  _EyeGlowColor("Eye Glow", Color) = (1,0.01,0,1)
  _Eye1("Eye 1", Vector) = (-0.27,0.14,0.72,0)
  _Eye2("Eye 2", Vector) = (0.27,0.14,0.72,0)
  _EyeGlowRadius("Glow Radius", Float) = 0.62
  _EyeGlowStrength("Glow Strength", Float) = 0.62
 }
 SubShader
 {
  Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
  Pass
  {
   Name "QuestSmoke"
   Tags { "LightMode"="UniversalForward" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST; half4 _Tint; half4 _EyeGlowColor; float4 _Eye1; float4 _Eye2; float _EyeGlowRadius; float _EyeGlowStrength;
   CBUFFER_END
   struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float3 positionOS:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
   Varyings vert(Attributes i){Varyings o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionHCS=TransformObjectToHClip(i.positionOS.xyz);o.positionOS=i.positionOS.xyz;o.uv=TRANSFORM_TEX(i.uv,_BaseMap);o.color=i.color;return o;}
   half4 frag(Varyings i):SV_Target{half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);float d=min(distance(i.positionOS,_Eye1.xyz),distance(i.positionOS,_Eye2.xyz));half glow=saturate(1-d/max(_EyeGlowRadius,.001));glow=glow*glow*_EyeGlowStrength;half3 baseCol=tex.rgb*i.color.rgb*_Tint.rgb;half alpha=tex.a*i.color.a*_Tint.a;half3 rgb=baseCol+_EyeGlowColor.rgb*glow*tex.a;return half4(rgb,alpha);}
   ENDHLSL
  }
 }
}