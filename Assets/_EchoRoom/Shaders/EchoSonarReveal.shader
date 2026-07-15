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
        _EnvironmentLightInfluence ("Realtime Light Influence", Range(0, 2)) = 0.85
        _EnvironmentAmbientInfluence ("Ambient Light Influence", Range(0, 1)) = 0.08
        _RealtimeLightResponse ("Realtime Light Response", Range(0, 4)) = 1.35
        _RealtimeLightCompression ("Realtime Light Compression", Range(0, 1)) = 0.2
        _AOVisibilityStrength ("AO Visibility Strength", Range(0, 1)) = 0.75

        [Header(Editor Preview)]
        [Toggle]_SceneViewPreview ("Reveal In Scene View", Float) = 0

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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define ECHO_MAX_PULSES 16

            // Global pulse buffer (written by SonarRevealController). Outside CBUFFER on purpose:
            // global arrays are not SRP-batcher compatible, which is fine for this FX shader.
            float4 _SonarPulses[ECHO_MAX_PULSES];
            float _EchoSceneViewCamera;
            float _EchoRevealLingerOverride;

            TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MetallicGlossMap);  SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_OcclusionMap);      SAMPLER(sampler_OcclusionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _EnvironmentLightInfluence;
                float _EnvironmentAmbientInfluence;
                float _RealtimeLightResponse;
                float _RealtimeLightCompression;
                float _AOVisibilityStrength;
                float _SceneViewPreview;
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
                float2 staticLightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                half fogFactor : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);
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
                OUT.fogFactor = ComputeFogFactor(p.positionCS.z);
                OUTPUT_LIGHTMAP_UV(IN.staticLightmapUV, unity_LightmapST, OUT.staticLightmapUV);
                OUTPUT_SH(OUT.normalWS, OUT.vertexSH);
                return OUT;
            }

            half4 frag (Varyings IN, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;

                // Ordinary scene lighting between sonar pulses.
                float environmentFaceSign = IS_FRONT_VFACE(frontFace, 1.0, -1.0);
                float3 environmentNormal = normalize(IN.normalWS) * environmentFaceSign;
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                AmbientOcclusionFactor screenSpaceAO = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(IN.positionHCS));

                // Baked GI replaces the ambient probe only inside the existing reveal path.
                // With no lightmap assigned, SAMPLE_GI falls back to the same vertex SH behavior.
                half3 bakedEnvironment = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, environmentNormal);
                half3 ambientContribution = bakedEnvironment * _EnvironmentAmbientInfluence *
                                            screenSpaceAO.indirectAmbientOcclusion;
                half3 realtimeLight = mainLight.color * saturate(dot(environmentNormal, mainLight.direction)) *
                                      mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                #if defined(_ADDITIONAL_LIGHTS)
                uint additionalLightCount = GetAdditionalLightsCount();
                [loop]
                for (uint lightIndex = 0u; lightIndex < additionalLightCount; ++lightIndex)
                {
                    Light sceneLight = GetAdditionalLight(lightIndex, IN.positionWS);
                    realtimeLight += sceneLight.color * saturate(dot(environmentNormal, sceneLight.direction)) *
                                     sceneLight.distanceAttenuation * sceneLight.shadowAttenuation;
                }
                #endif

                // Keep realtime lights responsive while retaining an optional soft cap for extreme values.
                half3 realtimeContribution = realtimeLight * _EnvironmentLightInfluence *
                                             screenSpaceAO.directAmbientOcclusion;
                realtimeContribution *= _RealtimeLightResponse;
                realtimeContribution = realtimeContribution / (1.0h + realtimeContribution * _RealtimeLightCompression);
                // The environment is completely black until revealAmt is produced by a ping.
                half3 baseCol = half3(0.0h, 0.0h, 0.0h);
                half3 geometricSceneLighting = ambientContribution + realtimeContribution;

                float t = _Time.y;
                float revealLingerSeconds = _EchoRevealLingerOverride > 0.0 ? _EchoRevealLingerOverride : _RevealLinger;
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

                    float maxAge = _RevealRadius / max(_RevealSpeed, 0.001) + revealLingerSeconds;
                    if (age > maxAge) continue;

                    float3 pulsePos = _SonarPulses[i].xyz;
                    float d = distance(IN.positionWS, pulsePos);
                    if (d > _RevealRadius + _RingWidth) continue;

                    float radius = age * _RevealSpeed;
                    float fromEdge = radius - d;            // > 0 once the wave has passed this point

                    if (fromEdge > 0.0 && d <= _RevealRadius)
                    {
                        float timeSince = fromEdge / max(_RevealSpeed, 0.001);
                        float linger = saturate(1.0 - timeSince / max(revealLingerSeconds, 0.001));
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
                    // ---- Flat: scene-lit albedo using geometric normals ----
                    lit = albedo * geometricSceneLighting * _RevealBrightness * tintMul;
                }
                else
                {
                    // ---- Lit modes: scene lighting evaluated with the material normal map ----
                    float faceSign = IS_FRONT_VFACE(frontFace, 1.0, -1.0);
                    float3 nWS = normalize(IN.normalWS) * faceSign;
                    float3 tWS = normalize(IN.tangentWS);
                    float3 bWS = normalize(IN.bitangentWS) * faceSign;
                    float3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                    float3 N = normalize(TransformTangentToWorld(nTS, float3x3(tWS, bWS, nWS)));

                    half3 normalBakedEnvironment = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, N);
                    half3 normalAmbient = normalBakedEnvironment * _EnvironmentAmbientInfluence *
                                          screenSpaceAO.indirectAmbientOcclusion;
                    half3 normalRealtime = mainLight.color * saturate(dot(N, mainLight.direction)) *
                                           mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                    #if defined(_ADDITIONAL_LIGHTS)
                    [loop]
                    for (uint normalLightIndex = 0u; normalLightIndex < additionalLightCount; ++normalLightIndex)
                    {
                        Light normalSceneLight = GetAdditionalLight(normalLightIndex, IN.positionWS);
                        normalRealtime += normalSceneLight.color * saturate(dot(N, normalSceneLight.direction)) *
                                          normalSceneLight.distanceAttenuation * normalSceneLight.shadowAttenuation;
                    }
                    #endif

                    half3 normalRealtimeContribution = normalRealtime * _EnvironmentLightInfluence *
                                                       screenSpaceAO.directAmbientOcclusion;
                    normalRealtimeContribution *= _RealtimeLightResponse;
                    normalRealtimeContribution = normalRealtimeContribution / (1.0h + normalRealtimeContribution * _RealtimeLightCompression);
                    half3 normalSceneLighting = normalAmbient + normalRealtimeContribution;

                    if (_RevealQuality < 1.5)
                    {
                        // ---- Normals: scene-lit relief, no specular ----
                        lit = albedo * normalSceneLighting * _RevealBrightness * tintMul;
                    }
                    else
                    {
                        // ---- PBR: relief + metallic/smoothness sheen + AO ----
                        half4 mg = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, IN.uv);
                        float metallic = _Metallic * mg.r;
                        float smoothness = _Smoothness * mg.a;
                        float occ = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, IN.uv).g, _OcclusionStrength);

                        float3 V = normalize(GetCameraPositionWS() - IN.positionWS);
                        float specPower = exp2(smoothness * 10.0 + 1.0);
                        half3 specColor = lerp((half3)0.04, albedo, metallic);

                        // Non-metals use baked/realtime diffuse. Metals receive the same baked
                        // environment through their specular color, so lightmapped iron is not black.
                        half3 diffuse = albedo * (1.0 - metallic) * normalSceneLighting;
                        half3 indirectSpecular = specColor * normalAmbient * metallic;

                        float3 L = normalize(mainLight.direction);
                        float ndl = saturate(dot(N, L));
                        float3 H = normalize(L + V);
                        float ndh = saturate(dot(N, H));
                        float spec = pow(ndh, specPower) * ndl;
                        half3 directSpecular = specColor * spec * mainLight.color *
                                               mainLight.distanceAttenuation * mainLight.shadowAttenuation *
                                               screenSpaceAO.directAmbientOcclusion;

                        // Maze lights are point lights, which URP supplies as additional lights.
                        // Include their specular response so metallic door frames react in realtime.
                        #if defined(_ADDITIONAL_LIGHTS)
                        [loop]
                        for (uint specularLightIndex = 0u; specularLightIndex < additionalLightCount; ++specularLightIndex)
                        {
                            Light specularLight = GetAdditionalLight(specularLightIndex, IN.positionWS);
                            float3 specularL = normalize(specularLight.direction);
                            float specularNdl = saturate(dot(N, specularL));
                            float3 specularH = normalize(specularL + V);
                            float specularNdh = saturate(dot(N, specularH));
                            float specularTerm = pow(specularNdh, specPower) * specularNdl;
                            directSpecular += specColor * specularTerm * specularLight.color *
                                              specularLight.distanceAttenuation * specularLight.shadowAttenuation *
                                              screenSpaceAO.directAmbientOcclusion;
                        }
                        #endif

                        lit = (diffuse + indirectSpecular + directSpecular) * occ * _RevealBrightness * tintMul;
                    }
                }

                // Fog and scene lighting exist only inside the sonar visibility mask.
                half3 revealedCol = MixFog(lit, IN.fogFactor);
                float finalAO = min(screenSpaceAO.indirectAmbientOcclusion, screenSpaceAO.directAmbientOcclusion);
                revealedCol *= lerp(1.0h, finalAO, saturate(_AOVisibilityStrength));
                float visibilityMask = max(saturate(revealAmt), saturate(_SceneViewPreview * _EchoSceneViewCamera));
                half3 col = lerp(baseCol, revealedCol, visibilityMask);
                col += _RingColor.rgb * _RingBrightness * saturate(ringGlow);
                return half4(col, 1.0);
            }
            ENDHLSL
        }

        // Bake-only pass: exposes the visible texture albedo to the lightmapper.
        // It does not participate in runtime rendering or alter the sonar visibility mask.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex metaVert
            #pragma fragment metaFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;

            struct MetaAttributes
            {
                float4 positionOS : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct MetaVaryings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            MetaVaryings metaVert(MetaAttributes IN)
            {
                MetaVaryings OUT;
                OUT.positionHCS = MetaVertexPosition(
                    IN.positionOS, IN.uv1, IN.uv2, unity_LightmapST, unity_DynamicLightmapST);
                OUT.uv = TRANSFORM_TEX(IN.uv0, _BaseMap);
                return OUT;
            }

            half4 metaFrag(MetaVaryings IN) : SV_Target
            {
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
                metaInput.Emission = half3(0.0h, 0.0h, 0.0h);
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }

        // Opaque URP passes let the maze cast realtime shadows and write depth.
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Universal Render Pipeline/Unlit"
}
