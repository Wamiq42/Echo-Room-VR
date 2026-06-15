// Echo Room VR - body-centered sonar pulse (Dying Light 2 "Survivor Sense" style).
//
// Each ping is an expanding spherical shell centered on the player. Any surface whose
// world-space distance from a ping origin falls inside the moving shell lights up with an
// animated, granular shimmer and a soft trailing afterglow. Up to ECHO_MAX_PINGS waves can
// be active at once.
//
// Ping data is fed in globally by EchoPulseController via Shader.SetGlobalVectorArray:
//   _PingOrigins[i].xyz = world-space origin of the ping (the player's body)
//   _PingOrigins[i].w   = start time in seconds (Time.timeSinceLevelLoad); <= 0 means "slot empty".
// The shell radius is derived on the GPU from elapsed time, so the CPU only writes once per ping.
Shader "EchoRoom/EchoPulseSonar"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color", Color) = (0.05, 0.09, 0.12, 1)

        [Header(Sonar Ring)]
        [HDR]_RingColor ("Ring Color (HDR)", Color) = (0.18, 1.4, 1.5, 1)
        _RingSpeed ("Ring Speed (m/s)", Float) = 6
        _RingWidth ("Ring Width (m)", Float) = 0.8
        _EdgeSoftness ("Edge Softness (m)", Float) = 0.25
        _MaxRadius ("Max Radius (m)", Float) = 18

        [Header(Shimmer)]
        _SparkleScale ("Sparkle Scale", Float) = 6
        _SparkleSpeed ("Sparkle Speed", Float) = 2
        _SparkleStrength ("Sparkle Strength", Range(0, 1)) = 0.7

        [Header(Afterglow)]
        _Afterglow ("Afterglow Strength", Range(0, 1)) = 0.15

        [Header(Proximity Reveal)]
        _RevealRadius ("Reveal Radius (m)", Float) = 7
        _RevealStrength ("Reveal Strength", Range(0, 3)) = 0.9
        _RevealTint ("Reveal Teal Tint", Range(0, 1)) = 0.2
        _RevealFade ("Reveal Fade Duration (s)", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define ECHO_MAX_PINGS 20

            // Global ping buffer (written by EchoPulseController). Outside CBUFFER on purpose:
            // global arrays are not SRP-batcher compatible, which is fine for this FX shader.
            float4 _PingOrigins[ECHO_MAX_PINGS];

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _RingColor;
                float _RingSpeed;
                float _RingWidth;
                float _EdgeSoftness;
                float _MaxRadius;
                float _SparkleScale;
                float _SparkleSpeed;
                float _SparkleStrength;
                float _Afterglow;
                float _RevealRadius;
                float _RevealStrength;
                float _RevealTint;
                float _RevealFade;
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

            // Cheap 3D value noise for the granular shimmer.
            float hash31 (float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float vnoise (float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash31(i + float3(0, 0, 0));
                float n100 = hash31(i + float3(1, 0, 0));
                float n010 = hash31(i + float3(0, 1, 0));
                float n110 = hash31(i + float3(1, 1, 0));
                float n001 = hash31(i + float3(0, 0, 1));
                float n101 = hash31(i + float3(1, 0, 1));
                float n011 = hash31(i + float3(0, 1, 1));
                float n111 = hash31(i + float3(1, 1, 1));

                float x00 = lerp(n000, n100, f.x);
                float x10 = lerp(n010, n110, f.x);
                float x01 = lerp(n001, n101, f.x);
                float x11 = lerp(n011, n111, f.x);
                float y0 = lerp(x00, x10, f.y);
                float y1 = lerp(x01, x11, f.y);
                return lerp(y0, y1, f.z);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
                half3 baseCol = albedo * _BaseColor.rgb;   // near-black ambient between pings
                half3 emission = (half3)0;
                float t = _Time.y;

                [loop]
                for (int i = 0; i < ECHO_MAX_PINGS; i++)
                {
                    float startT = _PingOrigins[i].w;
                    if (startT <= 0.0) continue;            // empty slot

                    float elapsed = t - startT;
                    if (elapsed < 0.0) continue;

                    float radius = elapsed * _RingSpeed;
                    if (radius > _MaxRadius + _RingWidth) continue;   // wave finished

                    float d = distance(IN.positionWS, _PingOrigins[i].xyz);
                    float fromEdge = radius - d;            // > 0 once the shell has swept past this point

                    // Band mask: surface distance within [radius - width, radius].
                    float lead = smoothstep(0.0, _EdgeSoftness, fromEdge);
                    float trail = 1.0 - smoothstep(_RingWidth - _EdgeSoftness, _RingWidth, fromEdge);
                    float band = lead * trail;

                    // Fade the whole wave out as it reaches max radius.
                    float life = saturate(1.0 - radius / _MaxRadius);

                    // Bright shimmering leading band.
                    if (band > 0.001)
                    {
                        float sp = vnoise(IN.positionWS * _SparkleScale + t * _SparkleSpeed);
                        sp = lerp(1.0, sp * sp, _SparkleStrength);
                        emission += _RingColor.rgb * band * life * sp;
                    }

                    // Soft afterglow trailing the leading edge (tight bright trail).
                    float afterTrail = 1.0 - smoothstep(_RingWidth, _RingWidth * 4.0, fromEdge);
                    emission += _RingColor.rgb * lead * afterTrail * life * _Afterglow;

                    // Proximity reveal: surfaces the wave has already swept light up with their
                    // OWN texture, brightest near the origin and fading back to dark over time.
                    // distFalloff caps the reveal to the nearby area; timeFade returns it to black.
                    if (fromEdge > 0.0)
                    {
                        float distFalloff = saturate(1.0 - d / max(_RevealRadius, 0.001));
                        float timeSincePassed = fromEdge / max(_RingSpeed, 0.001);   // seconds since wavefront reached here
                        float timeFade = saturate(1.0 - timeSincePassed / max(_RevealFade, 0.001));
                        float reveal = distFalloff * distFalloff * timeFade;          // squared = tighter, more local
                        emission += (albedo * _RevealStrength + _RingColor.rgb * _RevealTint) * reveal;
                    }
                }

                return half4(baseCol + emission, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
