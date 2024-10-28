Shader "ToonDynamic"
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
                // UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
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

            // static const float threshold_map[8][8] = {
            //     { 0.001, 0.762, 0.190, 0.952, 0.048, 0.810, 0.238, 1.000 },
            //     { 0.508, 0.254, 0.698, 0.444, 0.556, 0.302, 0.746, 0.492 },
            //     { 0.127, 0.889, 0.063, 0.825, 0.175, 0.937, 0.111, 0.873 },
            //     { 0.635, 0.381, 0.571, 0.317, 0.683, 0.429, 0.619, 0.365 },
            //     { 0.032, 0.794, 0.222, 0.984, 0.016, 0.778, 0.206, 0.968 },
            //     { 0.540, 0.286, 0.730, 0.476, 0.524, 0.270, 0.714, 0.460 },
            //     { 0.159, 0.921, 0.095, 0.857, 0.143, 0.905, 0.079, 0.841 },
            //     { 0.667, 0.413, 0.603, 0.349, 0.651, 0.397, 0.587, 0.333 }
            // };
            static const float threshold_map[16][16] = {
                { 0.001, 0.502, 0.125, 0.627, 0.031, 0.533, 0.157, 0.659, 0.008, 0.510, 0.133, 0.635, 0.039, 0.541, 0.165, 0.667 },
                { 0.753, 0.251, 0.878, 0.376, 0.784, 0.282, 0.910, 0.408, 0.761, 0.259, 0.886, 0.384, 0.792, 0.290, 0.918, 0.416 },
                { 0.188, 0.690, 0.063, 0.565, 0.220, 0.722, 0.094, 0.596, 0.196, 0.698, 0.071, 0.573, 0.227, 0.729, 0.102, 0.604 },
                { 0.941, 0.439, 0.816, 0.314, 0.973, 0.471, 0.847, 0.345, 0.949, 0.447, 0.824, 0.322, 0.980, 0.478, 0.855, 0.353 },
                { 0.047, 0.549, 0.173, 0.675, 0.016, 0.518, 0.141, 0.643, 0.055, 0.557, 0.180, 0.682, 0.024, 0.525, 0.149, 0.651 },
                { 0.800, 0.298, 0.925, 0.424, 0.769, 0.267, 0.894, 0.392, 0.808, 0.306, 0.933, 0.431, 0.776, 0.275, 0.902, 0.400 },
                { 0.235, 0.737, 0.110, 0.612, 0.204, 0.706, 0.078, 0.580, 0.243, 0.745, 0.118, 0.620, 0.212, 0.714, 0.086, 0.588 },
                { 0.988, 0.486, 0.863, 0.361, 0.957, 0.455, 0.831, 0.329, 0.996, 0.494, 0.871, 0.369, 0.965, 0.463, 0.839, 0.337 },
                { 0.012, 0.514, 0.137, 0.639, 0.043, 0.545, 0.169, 0.671, 0.004, 0.506, 0.129, 0.631, 0.035, 0.537, 0.161, 0.663 },
                { 0.765, 0.263, 0.890, 0.388, 0.796, 0.294, 0.922, 0.420, 0.757, 0.255, 0.882, 0.380, 0.788, 0.286, 0.914, 0.412 },
                { 0.200, 0.702, 0.071, 0.573, 0.227, 0.729, 0.102, 0.604, 0.192, 0.694, 0.067, 0.569, 0.224, 0.725, 0.098, 0.600 },
                { 0.953, 0.451, 0.827, 0.325, 0.984, 0.482, 0.855, 0.353, 0.945, 0.443, 0.820, 0.318, 0.976, 0.475, 0.847, 0.345 },
                { 0.059, 0.561, 0.184, 0.686, 0.027, 0.529, 0.153, 0.655, 0.051, 0.553, 0.176, 0.678, 0.020, 0.522, 0.145, 0.647 },
                { 0.812, 0.310, 0.937, 0.435, 0.780, 0.278, 0.906, 0.404, 0.804, 0.302, 0.929, 0.427, 0.773, 0.271, 0.898, 0.396 },
                { 0.247, 0.749, 0.122, 0.624, 0.216, 0.718, 0.090, 0.592, 0.239, 0.741, 0.114, 0.616, 0.208, 0.710, 0.082, 0.584 },
                { 1.0, 0.498, 0.875, 0.373, 0.969, 0.467, 0.843, 0.341, 0.992, 0.490, 0.867, 0.365, 0.961, 0.459, 0.835, 0.333 }
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
                    //smoothstep(0.0, 0.01, NdotLShadowed);
                    //0.333 * (smoothstep(0.0, 0.01, NdotLShadowed)
                    //+ smoothstep(0.333, 0.343, NdotLShadowed)
                    //+ smoothstep(0.667, 0.677, NdotLShadowed));
                    //0.25 * (smoothstep(0.0, 0.01, NdotLShadowed)
                    //+ smoothstep(0.25, 0.26, NdotLShadowed)
                    //+ smoothstep(0.5, 0.51, NdotLShadowed)
                    //+ smoothstep(0.75, 0.76, NdotLShadowed));
                    //0.2 * (smoothstep(0.0, 0.01, NdotLShadowed)
                    //+ smoothstep(0.2, 0.21, NdotLShadowed)
                    //+ smoothstep(0.4, 0.41, NdotLShadowed)
                    //+ smoothstep(0.6, 0.61, NdotLShadowed)
                    //+ smoothstep(0.8, 0.81, NdotLShadowed));
                    0.167 * (smoothstep(0.0, 0.01, NdotLShadowed)
                    + smoothstep(0.167, 0.177, NdotLShadowed)
                    + smoothstep(0.333, 0.343, NdotLShadowed)
                    + smoothstep(0.5, 0.51, NdotLShadowed)
                    + smoothstep(0.667, 0.677, NdotLShadowed)
                    + smoothstep(0.833, 0.843, NdotLShadowed));
                half3 lightColor = mainLight.color * lightIntensity;

                // Specular reflection
                float3 halfVector = normalize(mainLight.direction + viewDirWS);
                float NdotH = dot(normalWS, halfVector);
                float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth =
                    //smoothstep(0.005, 0.01, specularIntensity);
                    //0.25 * (smoothstep(0.0, 0.01, specularIntensity)
                    //+ smoothstep(0.25, 0.26, specularIntensity)
                    //+ smoothstep(0.5, 0.51, specularIntensity)
                    //+ smoothstep(0.75, 0.76, specularIntensity));
                    0.167 * (smoothstep(0.0, 0.01, specularIntensity)
                    + smoothstep(0.167, 0.177, specularIntensity)
                    + smoothstep(0.333, 0.343, specularIntensity)
                    + smoothstep(0.5, 0.51, specularIntensity)
                    + smoothstep(0.667, 0.677, specularIntensity)
                    + smoothstep(0.833, 0.843, specularIntensity));
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

                int xRemap = screenUV.x * 1024 % 16;
                int yRemap = screenUV.y * 1024 % 16;
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

        //Pass
        //{
        //    // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
        //    // no LightMode tag are also rendered by Universal Render Pipeline
        //    Name "GBuffer"
        //    Tags
        //    {
        //        "LightMode" = "UniversalGBuffer"
        //    }
        //
        //    // -------------------------------------
        //    // Render State Commands
        //    ZWrite[_ZWrite]
        //    ZTest LEqual
        //    Cull[_Cull]
        //
        //    HLSLPROGRAM
        //    #pragma target 4.5
        //
        //    // Deferred Rendering Path does not support the OpenGL-based graphics API:
        //    // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
        //    #pragma exclude_renderers gles3 glcore
        //
        //    // -------------------------------------
        //    // Shader Stages
        //    #pragma vertex LitGBufferPassVertex
        //    #pragma fragment LitGBufferPassFragment
        //
        //    // -------------------------------------
        //    // Material Keywords
        //    #pragma shader_feature_local _NORMALMAP
        //    #pragma shader_feature_local_fragment _ALPHATEST_ON
        //    //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
        //    #pragma shader_feature_local_fragment _EMISSION
        //    #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
        //    #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //    #pragma shader_feature_local_fragment _OCCLUSIONMAP
        //    #pragma shader_feature_local _PARALLAXMAP
        //    #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
        //
        //    #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
        //    #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
        //    #pragma shader_feature_local_fragment _SPECULAR_SETUP
        //    #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
        //
        //    // -------------------------------------
        //    // Universal Pipeline keywords
        //    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        //    //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        //    //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        //    #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        //    #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        //    #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        //    #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        //    #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        //    #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        //
        //    // -------------------------------------
        //    // Unity defined keywords
        //    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        //    #pragma multi_compile _ SHADOWS_SHADOWMASK
        //    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        //    #pragma multi_compile _ LIGHTMAP_ON
        //    #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        //    #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
        //    #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        //
        //    //--------------------------------------
        //    // GPU Instancing
        //    #pragma multi_compile_instancing
        //    #pragma instancing_options renderinglayer
        //    #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        //
        //    // -------------------------------------
        //    // Includes
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl"
        //    ENDHLSL
        //}
        //
        //Pass
        //{
        //    Name "DepthOnly"
        //    Tags
        //    {
        //        "LightMode" = "DepthOnly"
        //    }
        //
        //    // -------------------------------------
        //    // Render State Commands
        //    ZWrite On
        //    ColorMask R
        //    Cull[_Cull]
        //
        //    HLSLPROGRAM
        //    #pragma target 2.0
        //
        //    // -------------------------------------
        //    // Shader Stages
        //    #pragma vertex DepthOnlyVertex
        //    #pragma fragment DepthOnlyFragment
        //
        //    // -------------------------------------
        //    // Material Keywords
        //    #pragma shader_feature_local _ALPHATEST_ON
        //    #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //
        //    // -------------------------------------
        //    // Unity defined keywords
        //    #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
        //
        //    //--------------------------------------
        //    // GPU Instancing
        //    #pragma multi_compile_instancing
        //    #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        //
        //    // -------------------------------------
        //    // Includes
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
        //    ENDHLSL
        //}

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