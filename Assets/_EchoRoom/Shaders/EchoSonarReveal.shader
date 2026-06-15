// Echo Room VR - expanding sonar reveal.
//
// Each ping sends a bright ring expanding outward from the player's origin. As the
// wavefront passes a surface, that surface lights up showing its REAL texture (radial
// falloff, capped at _RevealRadius), lingers briefly, then fades back to black. The
// leading edge is drawn as a glowing ring so you see the "circle" travelling out from you.
//
// Pulse data is fed in globally by SonarRevealController via Shader.SetGlobalVectorArray:
//   _SonarPulses[i].xyz = world-space origin of the ping (the player's body)
//   _SonarPulses[i].w   = start time in seconds (Time.timeSinceLevelLoad); <= 0 means "slot empty".
Shader "EchoRoom/EchoSonarReveal"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color (dark ambient)", Color) = (0.03, 0.04, 0.05, 1)

        [Header(Sonar Pulse)]
        _RevealSpeed ("Expand Speed (m/s)", Float) = 8
        _RevealRadius ("Max Reveal Radius (m)", Float) = 8
        _RevealLinger ("Area Linger (s)", Float) = 1.5

        [Header(Leading Ring)]
        [HDR]_RingColor ("Ring Color (HDR)", Color) = (0.2, 1.4, 1.5, 1)
        _RingWidth ("Ring Width (m)", Float) = 0.6
        _RingBrightness ("Ring Brightness", Range(0, 4)) = 1.5

        [Header(Revealed Surface)]
        _RevealBrightness ("Reveal Brightness", Range(0, 4)) = 1.2
        [HDR]_RevealTint ("Reveal Tint (HDR)", Color) = (0.5, 1.0, 1.05, 1)
        _TintAmount ("Tint Amount", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off   // render both sides so ceilings / outward-facing geometry stay visible from inside

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define ECHO_MAX_PULSES 16

            // Global pulse buffer (written by SonarRevealController). Outside CBUFFER on purpose:
            // global arrays are not SRP-batcher compatible, which is fine for this FX shader.
            float4 _SonarPulses[ECHO_MAX_PULSES];

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _RevealSpeed;
                float _RevealRadius;
                float _RevealLinger;
                half4 _RingColor;
                float _RingWidth;
                float _RingBrightness;
                float _RevealBrightness;
                half4 _RevealTint;
                float _TintAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positions.positionCS;
                OUT.positionWS = positions.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
                half3 baseCol = albedo * _BaseColor.rgb;   // near-black ambient between pings

                float t = _Time.y;
                float revealAmt = 0.0;   // how much the swept area is revealed (shows real colour)
                float ringGlow = 0.0;    // bright leading edge (the travelling circle)

                [loop]
                for (int i = 0; i < ECHO_MAX_PULSES; i++)
                {
                    float startT = _SonarPulses[i].w;
                    if (startT <= 0.0) continue;            // empty slot

                    float age = t - startT;
                    if (age < 0.0) continue;

                    // Lifetime: wave travels to _RevealRadius, then the lingering reveal fades.
                    float maxAge = _RevealRadius / max(_RevealSpeed, 0.001) + _RevealLinger;
                    if (age > maxAge) continue;

                    float d = distance(IN.positionWS, _SonarPulses[i].xyz);
                    if (d > _RevealRadius + _RingWidth) continue;

                    float radius = age * _RevealSpeed;      // current wavefront radius
                    float fromEdge = radius - d;            // > 0 once the wave has passed this point

                    // Area reveal behind the wavefront: lights up real texture, lingers, fades.
                    if (fromEdge > 0.0 && d <= _RevealRadius)
                    {
                        float timeSince = fromEdge / max(_RevealSpeed, 0.001);          // s since wave reached here
                        float linger = saturate(1.0 - timeSince / max(_RevealLinger, 0.001));
                        float distFall = saturate(1.0 - d / max(_RevealRadius, 0.001));
                        revealAmt = max(revealAmt, linger * distFall);
                    }

                    // Bright leading ring at d ~= radius (the expanding circle you see emit from you).
                    float ring = 1.0 - smoothstep(0.0, _RingWidth, abs(fromEdge));
                    float ringLife = saturate(1.0 - radius / max(_RevealRadius, 0.001));
                    ringGlow += ring * ringLife;
                }

                // Revealed surface shows its REAL texture colour (subtle multiplicative tint).
                half3 tintMul = lerp(half3(1.0, 1.0, 1.0), _RevealTint.rgb, _TintAmount);
                half3 lit = albedo * _RevealBrightness * tintMul;
                half3 col = lerp(baseCol, lit, saturate(revealAmt));

                // Add the glowing leading ring on top.
                col += _RingColor.rgb * _RingBrightness * saturate(ringGlow);

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
