Shader "Toon"
{
    Properties
    {
        _Color("Color", Color) = (0.5, 0.65, 1, 1)
        _Opacity("Opacity", Range(0, 1)) = 1
        [MainTexture] _BaseMap("Main Texture", 2D) = "white" {}
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
        [HDR] _SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness("Glossiness", Float) = 32
        [HDR] _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
        // _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        // _OutlineThreshold ("Outline Threshold", Range(0, 1)) = 0.1
        // _DepthSensitivity ("Depth Sensitivity", Range(0, 1)) = 0.1
        // _NormalSensitivity ("Normal Sensitivity", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
            UNITY_INSTANCING_BUFFER_END(Props)

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Color;
                half4 _AmbientColor;
                half4 _SpecularColor;
                float _Glossiness;
                half4 _RimColor;
                float _RimAmount;
                float _RimThreshold;
                // half4 _OutlineColor;
                // float _OutlineThreshold;
                // float _DepthSensitivity;
                // float _NormalSensitivity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            // static float threshold_map[8][8] = {
            //     { 0.001, 0.762, 0.190, 0.952, 0.048, 0.810, 0.238, 1.000 },
            //     { 0.508, 0.254, 0.698, 0.444, 0.556, 0.302, 0.746, 0.492 },
            //     { 0.127, 0.889, 0.063, 0.825, 0.175, 0.937, 0.111, 0.873 },
            //     { 0.635, 0.381, 0.571, 0.317, 0.683, 0.429, 0.619, 0.365 },
            //     { 0.032, 0.794, 0.222, 0.984, 0.016, 0.778, 0.206, 0.968 },
            //     { 0.540, 0.286, 0.730, 0.476, 0.524, 0.270, 0.714, 0.460 },
            //     { 0.159, 0.921, 0.095, 0.857, 0.143, 0.905, 0.079, 0.841 },
            //     { 0.667, 0.413, 0.603, 0.349, 0.651, 0.397, 0.587, 0.333 }
            // };
            static float threshold_map[16][16] = {
                { 0.001 / 255.0, 128.0 / 255.0, 32.0 / 255.0, 160.0 / 255.0, 8.0 / 255.0, 136.0 / 255.0, 40.0 / 255.0, 168.0 / 255.0, 2.0 / 255.0, 130.0 / 255.0, 34.0 / 255.0, 162.0 / 255.0, 10.0 / 255.0, 138.0 / 255.0, 42.0 / 255.0, 170.0 / 255.0 },
                { 192.0 / 255.0, 64.0 / 255.0, 224.0 / 255.0, 96.0 / 255.0, 200.0 / 255.0, 72.0 / 255.0, 232.0 / 255.0, 104.0 / 255.0, 194.0 / 255.0, 66.0 / 255.0, 226.0 / 255.0, 98.0 / 255.0, 202.0 / 255.0, 74.0 / 255.0, 234.0 / 255.0, 106.0 / 255.0 },
                { 48.0 / 255.0, 176.0 / 255.0, 16.0 / 255.0, 144.0 / 255.0, 56.0 / 255.0, 184.0 / 255.0, 24.0 / 255.0, 152.0 / 255.0, 50.0 / 255.0, 178.0 / 255.0, 18.0 / 255.0, 146.0 / 255.0, 58.0 / 255.0, 186.0 / 255.0, 26.0 / 255.0, 154.0 / 255.0 },
                { 240.0 / 255.0, 112.0 / 255.0, 208.0 / 255.0, 80.0 / 255.0, 248.0 / 255.0, 120.0 / 255.0, 216.0 / 255.0, 88.0 / 255.0, 242.0 / 255.0, 114.0 / 255.0, 210.0 / 255.0, 82.0 / 255.0, 250.0 / 255.0, 122.0 / 255.0, 218.0 / 255.0, 90.0 / 255.0 },
                { 12.0 / 255.0, 140.0 / 255.0, 44.0 / 255.0, 172.0 / 255.0, 4.0 / 255.0, 132.0 / 255.0, 36.0 / 255.0, 164.0 / 255.0, 14.0 / 255.0, 142.0 / 255.0, 46.0 / 255.0, 174.0 / 255.0, 6.0 / 255.0, 134.0 / 255.0, 38.0 / 255.0, 166.0 / 255.0 },
                { 204.0 / 255.0, 76.0 / 255.0, 236.0 / 255.0, 108.0 / 255.0, 196.0 / 255.0, 68.0 / 255.0, 228.0 / 255.0, 100.0 / 255.0, 206.0 / 255.0, 78.0 / 255.0, 238.0 / 255.0, 110.0 / 255.0, 198.0 / 255.0, 70.0 / 255.0, 230.0 / 255.0, 102.0 / 255.0 },
                { 60.0 / 255.0, 188.0 / 255.0, 28.0 / 255.0, 156.0 / 255.0, 52.0 / 255.0, 180.0 / 255.0, 20.0 / 255.0, 148.0 / 255.0, 62.0 / 255.0, 190.0 / 255.0, 30.0 / 255.0, 158.0 / 255.0, 54.0 / 255.0, 182.0 / 255.0, 22.0 / 255.0, 150.0 / 255.0 },
                { 252.0 / 255.0, 124.0 / 255.0, 220.0 / 255.0, 92.0 / 255.0, 244.0 / 255.0, 116.0 / 255.0, 212.0 / 255.0, 84.0 / 255.0, 254.0 / 255.0, 126.0 / 255.0, 222.0 / 255.0, 94.0 / 255.0, 246.0 / 255.0, 118.0 / 255.0, 214.0 / 255.0, 86.0 / 255.0 },
                { 3.0 / 255.0, 131.0 / 255.0, 35.0 / 255.0, 163.0 / 255.0, 11.0 / 255.0, 139.0 / 255.0, 43.0 / 255.0, 171.0 / 255.0, 1.0 / 255.0, 129.0 / 255.0, 33.0 / 255.0, 161.0 / 255.0, 9.0 / 255.0, 137.0 / 255.0, 41.0 / 255.0, 169.0 / 255.0 },
                { 195.0 / 255.0, 67.0 / 255.0, 227.0 / 255.0, 99.0 / 255.0, 203.0 / 255.0, 75.0 / 255.0, 235.0 / 255.0, 107.0 / 255.0, 193.0 / 255.0, 65.0 / 255.0, 225.0 / 255.0, 97.0 / 255.0, 201.0 / 255.0, 73.0 / 255.0, 233.0 / 255.0, 105.0 / 255.0 },
                { 51.0 / 255.0, 179.0 / 255.0, 19.0 / 255.0, 147.0 / 255.0, 59.0 / 255.0, 187.0 / 255.0, 27.0 / 255.0, 155.0 / 255.0, 49.0 / 255.0, 177.0 / 255.0, 17.0 / 255.0, 145.0 / 255.0, 57.0 / 255.0, 185.0 / 255.0, 25.0 / 255.0, 153.0 / 255.0 },
                { 243.0 / 255.0, 115.0 / 255.0, 211.0 / 255.0, 83.0 / 255.0, 251.0 / 255.0, 123.0 / 255.0, 219.0 / 255.0, 91.0 / 255.0, 241.0 / 255.0, 113.0 / 255.0, 209.0 / 255.0, 81.0 / 255.0, 249.0 / 255.0, 121.0 / 255.0, 217.0 / 255.0, 89.0 / 255.0 },
                { 15.0 / 255.0, 143.0 / 255.0, 47.0 / 255.0, 175.0 / 255.0, 7.0 / 255.0, 135.0 / 255.0, 39.0 / 255.0, 167.0 / 255.0, 13.0 / 255.0, 141.0 / 255.0, 45.0 / 255.0, 173.0 / 255.0, 5.0 / 255.0, 133.0 / 255.0, 37.0 / 255.0, 165.0 / 255.0 },
                { 207.0 / 255.0, 79.0 / 255.0, 239.0 / 255.0, 111.0 / 255.0, 199.0 / 255.0, 71.0 / 255.0, 231.0 / 255.0, 103.0 / 255.0, 205.0 / 255.0, 77.0 / 255.0, 237.0 / 255.0, 109.0 / 255.0, 197.0 / 255.0, 69.0 / 255.0, 229.0 / 255.0, 101.0 / 255.0 },
                { 63.0 / 255.0, 191.0 / 255.0, 31.0 / 255.0, 159.0 / 255.0, 55.0 / 255.0, 183.0 / 255.0, 23.0 / 255.0, 151.0 / 255.0, 61.0 / 255.0, 189.0 / 255.0, 29.0 / 255.0, 157.0 / 255.0, 53.0 / 255.0, 181.0 / 255.0, 21.0 / 255.0, 149.0 / 255.0 },
                { 255.0 / 255.0, 127.0 / 255.0, 223.0 / 255.0, 95.0 / 255.0, 247.0 / 255.0, 119.0 / 255.0, 215.0 / 255.0, 87.0 / 255.0, 253.0 / 255.0, 125.0 / 255.0, 221.0 / 255.0, 93.0 / 255.0, 245.0 / 255.0, 117.0 / 255.0, 213.0 / 255.0, 85.0 / 255.0 }
            };

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                // Shadow coords
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                // Main light
                Light mainLight = GetMainLight(shadowCoord);
                float NdotL = dot(normalWS, mainLight.direction);
                float shadowStepped = smoothstep(0.5, 0.51, mainLight.shadowAttenuation);
                float NdotLShadowed = NdotL * shadowStepped;
                float lightIntensity = 
                    // smoothstep(0.0, 0.01, NdotLShadowed);
                    // 0.3333333 * smoothstep(0.0, 0.01, NdotLShadowed)
                    // + 0.3333333 * smoothstep(0.3333333, 0.3433333, NdotLShadowed)
                    // + 0.3333333 * smoothstep(0.6666666, 0.6766666, NdotLShadowed);
                    // 0.25 * smoothstep(0.0, 0.01, NdotLShadowed)
                    // + 0.25 * smoothstep(0.25, 0.26, NdotLShadowed)
                    // + 0.25 * smoothstep(0.5, 0.51, NdotLShadowed)
                    // + 0.25 * smoothstep(0.75, 0.76, NdotLShadowed);
                    // 0.2 * smoothstep(0.0, 0.01, NdotLShadowed)
                    // + 0.2 * smoothstep(0.2, 0.21, NdotLShadowed)
                    // + 0.2 * smoothstep(0.4, 0.41, NdotLShadowed)
                    // + 0.2 * smoothstep(0.6, 0.61, NdotLShadowed)
                    // + 0.2 * smoothstep(0.8, 0.81, NdotLShadowed);
                    0.1666666 * smoothstep(0.0, 0.01, NdotLShadowed)
                    + 0.1666666 * smoothstep(0.1666666, 0.1676666, NdotLShadowed)
                    + 0.1666666 * smoothstep(0.3333333, 0.3433333, NdotLShadowed)
                    + 0.1666666 * smoothstep(0.5, 0.51, NdotLShadowed)
                    + 0.1666666 * smoothstep(0.6666666, 0.6766666, NdotLShadowed)
                    + 0.1666666 * smoothstep(0.8333333, 0.8433333, NdotLShadowed);
                half3 lightColor = mainLight.color * lightIntensity;

                // Specular reflection
                float3 halfVector = normalize(mainLight.direction + viewDirWS);
                float NdotH = dot(normalWS, halfVector);
                float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth =
                    // smoothstep(0.005, 0.01, specularIntensity);
                    // 0.25 * smoothstep(0.0, 0.01, specularIntensity)
                    // + 0.25 * smoothstep(0.25, 0.26, specularIntensity)
                    // + 0.25 * smoothstep(0.5, 0.51, specularIntensity)
                    // + 0.25 * smoothstep(0.75, 0.76, specularIntensity);
                    0.1666666 * smoothstep(0.0, 0.01, specularIntensity)
                    + 0.1666666 * smoothstep(0.1666666, 0.1676666, specularIntensity)
                    + 0.1666666 * smoothstep(0.3333333, 0.3433333, specularIntensity)
                    + 0.1666666 * smoothstep(0.5, 0.51, specularIntensity)
                    + 0.1666666 * smoothstep(0.6666666, 0.6766666, specularIntensity)
                    + 0.1666666 * smoothstep(0.8333333, 0.8433333, specularIntensity);
                half3 specular = specularIntensitySmooth * _SpecularColor.rgb;

                // Rim lighting
                float rimDot = 1 - dot(viewDirWS, normalWS);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                half3 rim = rimIntensity * _RimColor.rgb;

                // Final color
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 finalColor = _Color.rgb * baseMap.rgb * (_AmbientColor.rgb + lightColor + specular + rim);

                // Sample depth and normals
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                /*
                float depth = SampleSceneDepth(screenUV);
                float3 normals = SampleSceneNormals(screenUV);

                // Calculate depth and normal differences
                float2 texelSize = 1.0 / _ScreenParams.xy;
                float depthDiff = abs(depth - SampleSceneDepth(screenUV + float2(1,1) * texelSize));
                float3 normalDiff = abs(normals - SampleSceneNormals(screenUV + float2(1,1) * texelSize));

                // Detect edges
                float edgeDepth = depthDiff > _DepthSensitivity;
                float edgeNormal = length(normalDiff) > _NormalSensitivity;
                float edge = max(edgeDepth, edgeNormal);
                finalColor = lerp(finalColor, _OutlineColor, edge * _OutlineThreshold);
                */

                int xRemap = (int)(screenUV.x * 10000) % 16;
                int yRemap = (int)(screenUV.y * 10000) % 16;
                float threshold = threshold_map[xRemap][yRemap];
                clip(_Opacity - threshold);

                return half4(finalColor, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite[_ZWrite]
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitGBufferPassVertex
            #pragma fragment LitGBufferPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
}