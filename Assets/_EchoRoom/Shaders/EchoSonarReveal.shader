// Echo Room VR - expanding sonar reveal.
//
// Each ping sends a bright ring expanding outward from the player's origin. As the
// wavefront passes a surface, that surface lights up (radial falloff, capped at
// _RevealRadius), lingers briefly, then fades back to black. The leading edge is drawn
// as a glowing ring so you see the "circle" travelling out from you.
//
// _RevealQuality picks how the revealed surface is shaded (uniform branch, ~free):
//   0 Flat    - albedo only (cheapest; flat look).
//   1 Normals - normal map + the pulse acts as a moving light, so relief pops at the ring.
//   2 PBR     - normal + metallic/smoothness + AO, lit by the pulse (relief + sheen).
//
// Pulse data is fed in globally by SonarRevealController via Shader.SetGlobalVectorArray:
//   _SonarPulses[i].xyz = world-space origin of the ping (the player's body)
//   _SonarPulses[i].w   = start time in seconds (Time.timeSinceLevelLoad); <= 0 means "slot empty".
Shader "EchoRoom/EchoSonarReveal"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color (dark ambient)", Color) = (0.004, 0.0045, 0.006, 1)

        [Header(Surface Detail)]
        [Enum(Flat,0,Normals,1,PBR,2)] _RevealQuality ("Reveal Quality", Float) = 0
        _RevealAmbient ("Reveal Ambient (lit modes)", Range(0, 1)) = 0.35
        [Normal][NoScaleOffset]_BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        [NoScaleOffset]_MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [NoScaleOffset]_OcclusionMap ("Occlusion (G)", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1

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

            TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MetallicGlossMap);  SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_OcclusionMap);      SAMPLER(sampler_OcclusionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _RevealQuality;
                float _RevealAmbient;
                float _BumpScale;
                float _Metallic;
                float _Smoothness;
                float _OcclusionStrength;
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
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.positionHCS = p.positionCS;
                OUT.positionWS = p.positionWS;
                OUT.normalWS = n.normalWS;
                OUT.tangentWS = n.tangentWS;
                OUT.bitangentWS = n.bitangentWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag (Varyings IN, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
                half3 baseCol = albedo * _BaseColor.rgb;   // near-black ambient between pings

                float t = _Time.y;
                float revealAmt = 0.0;                      // strongest area-reveal contribution
                float3 lightPos = IN.positionWS + float3(0, 1, 0);  // origin of the dominant pulse (fallback above)
                float ringGlow = 0.0;

                [loop]
                for (int i = 0; i < ECHO_MAX_PULSES; i++)
                {
                    float startT = _SonarPulses[i].w;
                    if (startT <= 0.0) continue;            // empty slot

                    float age = t - startT;
                    if (age < 0.0) continue;

                    float maxAge = _RevealRadius / max(_RevealSpeed, 0.001) + _RevealLinger;
                    if (age > maxAge) continue;

                    float3 pulsePos = _SonarPulses[i].xyz;
                    float d = distance(IN.positionWS, pulsePos);
                    if (d > _RevealRadius + _RingWidth) continue;

                    float radius = age * _RevealSpeed;
                    float fromEdge = radius - d;            // > 0 once the wave has passed this point

                    if (fromEdge > 0.0 && d <= _RevealRadius)
                    {
                        float timeSince = fromEdge / max(_RevealSpeed, 0.001);
                        float linger = saturate(1.0 - timeSince / max(_RevealLinger, 0.001));
                        float distFall = saturate(1.0 - d / max(_RevealRadius, 0.001));
                        float c = linger * distFall;
                        if (c > revealAmt) { revealAmt = c; lightPos = pulsePos; }  // track dominant pulse as the light
                    }

                    float ring = 1.0 - smoothstep(0.0, _RingWidth, abs(fromEdge));
                    float ringLife = saturate(1.0 - radius / max(_RevealRadius, 0.001));
                    ringGlow += ring * ringLife;
                }

                half3 tintMul = lerp(half3(1.0, 1.0, 1.0), _RevealTint.rgb, _TintAmount);
                half3 lit;

                if (_RevealQuality < 0.5)
                {
                    // ---- Flat: albedo only ----
                    lit = albedo * _RevealBrightness * tintMul;
                }
                else
                {
                    // ---- Lit modes: the pulse is the light source ----
                    float faceSign = IS_FRONT_VFACE(frontFace, 1.0, -1.0);
                    float3 nWS = normalize(IN.normalWS) * faceSign;
                    float3 tWS = normalize(IN.tangentWS);
                    float3 bWS = normalize(IN.bitangentWS) * faceSign;
                    float3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                    float3 N = normalize(TransformTangentToWorld(nTS, float3x3(tWS, bWS, nWS)));

                    float3 L = normalize(lightPos - IN.positionWS);
                    float ndl = saturate(dot(N, L));
                    float shade = lerp(_RevealAmbient, 1.0, ndl);

                    if (_RevealQuality < 1.5)
                    {
                        // ---- Normals: relief, no specular ----
                        lit = albedo * shade * _RevealBrightness * tintMul;
                    }
                    else
                    {
                        // ---- PBR: relief + metallic/smoothness sheen + AO ----
                        half4 mg = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, IN.uv);
                        float metallic = _Metallic * mg.r;
                        float smoothness = _Smoothness * mg.a;
                        float occ = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, IN.uv).g, _OcclusionStrength);

                        float3 V = normalize(GetCameraPositionWS() - IN.positionWS);
                        float3 H = normalize(L + V);
                        float ndh = saturate(dot(N, H));
                        float specPower = exp2(smoothness * 10.0 + 1.0);
                        float spec = pow(ndh, specPower) * ndl;
                        half3 specColor = lerp((half3)0.04, albedo, metallic);

                        half3 diffuse = albedo * (1.0 - metallic) * shade;
                        lit = (diffuse + specColor * spec) * occ * _RevealBrightness * tintMul;
                    }
                }

                half3 col = lerp(baseCol, lit, saturate(revealAmt));
                col += _RingColor.rgb * _RingBrightness * saturate(ringGlow);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
