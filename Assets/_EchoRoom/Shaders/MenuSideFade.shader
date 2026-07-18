Shader "EchoRoom/UI/MenuSideFade"
{
    Properties
    {
        _Color ("Fade Color", Color) = (0,0,0,1)
        _MaxAlpha ("Max Alpha", Range(0,1)) = 0.78
        _Invert ("Invert Direction", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+600" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _MaxAlpha;
            float _Invert;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float edge = (_Invert > 0.5) ? i.uv.x : (1.0 - i.uv.x);
                edge = smoothstep(0.0, 1.0, saturate(edge));
                fixed4 c = _Color;
                c.a *= edge * _MaxAlpha;
                return c;
            }
            ENDCG
        }
    }
}
