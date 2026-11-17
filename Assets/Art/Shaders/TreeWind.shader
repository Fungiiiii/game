// URP Lit, with foliage that moves in the wind while trunks and branches stay
// still. Every pass is URP's own: a wrapper moves the vertex first, then hands it to
// the Lit vertex function unchanged, so the trees keep exactly the look of
// Palette.mat and the shadows of the foliage follow its motion.
//
// Two layers, as in the classic split between main and detail bending (Crysis,
// GPU Gems 3 ch. 16): the whole crown sways slowly downwind, and a slow wave runs
// across it so each clump moves a little on its own. The gust heard in the sound
// (_WindGust, see Wind.hlsl) pushes the foliage further downwind.
//
// What is wood is read from the palette cell the UV points at (Wood, Wood dark,
// Bark pale): the models carry no wind data of their own. Foliage moves as nearly
// rigid clumps, so the faceted low poly shapes stay crisp.
Shader "Fungiiiii/TreeWind"
{
    Properties
    {
        [Header(Wind)]
        _TreeSway ("Crown sway (m)", Range(0, 1)) = 0.12
        _TreeRipple ("Clump ripple (m)", Range(0, 0.5)) = 0.06
        _TreeGustLean ("Extra lean in a heard gust (m)", Range(0, 1)) = 0.3
        _TreeWindSpeed ("Speed", Range(0, 3)) = 0.6
        _TreeFlutter ("Leaf flutter (m)", Range(0, 0.1)) = 0.015

        [Header(Surface)]
        [MainTexture] _BaseMap ("Palette", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SpecColor ("Specular", Color) = (0.2, 0.2, 0.2)
        _OcclusionStrength ("Occlusion", Range(0, 1)) = 1
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _BumpScale ("Bump Scale", Float) = 1
        [HideInInspector] _Parallax ("Parallax", Float) = 0.005
        [HideInInspector] _EmissionColor ("Emission", Color) = (0, 0, 0)
        [HideInInspector] _DetailAlbedoMapScale ("Detail Albedo Scale", Float) = 1
        [HideInInspector] _DetailNormalMapScale ("Detail Normal Scale", Float) = 1
        [HideInInspector] _ClearCoatMask ("Clear Coat Mask", Float) = 0
        [HideInInspector] _ClearCoatSmoothness ("Clear Coat Smoothness", Float) = 0
        [HideInInspector] _Surface ("Surface", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Wind.hlsl"

        // Outside UnityPerMaterial, which LitInput.hlsl declares and owns: these
        // make the shader incompatible with the SRP Batcher, not with GPU instancing,
        // which is how the Terrain draws its trees.
        half _TreeSway;
        half _TreeRipple;
        half _TreeGustLean;
        half _TreeWindSpeed;
        half _TreeFlutter;

        // 1 for foliage, 0 for wood. Palette cells are (row, col) on an 8 x 8 grid,
        // as in tools/palette/generate_palette.py; UVs point at cell centres.
        float FoliageWeight(float2 uv)
        {
            int col = (int)floor(uv.x * 8);
            int row = (int)floor((1 - uv.y) * 8);
            bool wood = (row == 3 && (col == 2 || col == 3)) || (row == 6 && col == 4);
            return wood ? 0 : 1;
        }

        // Call after UNITY_SETUP_INSTANCE_ID, so the matrices are the tree's own.
        float3 TreeWindPositionOS(float3 positionOS, float2 uv)
        {
            float foliage = FoliageWeight(uv);
            if (foliage == 0)
            {
                return positionOS;
            }

            float3 positionWS = TransformObjectToWorld(positionOS);
            float3 rootWS = TransformObjectToWorld(float3(0, 0, 0));

            // Higher foliage is on thinner wood: it moves more than the lowest tiers.
            float reach = 0.5 + 0.5 * saturate((positionWS.y - rootWS.y) / 20);

            // Main: one phase per tree, so neighbours do not sway in step.
            float phase = _Time.y * _TreeWindSpeed + dot(rootWS.xz, float2(0.13, 0.17));
            float sway = _TreeSway * (0.5 + 0.5 * sin(phase));

            // Detail: a long, slow wave across the crown. Points of one clump are close,
            // so they move together; clumps metres apart move out of step.
            float ripplePhase = phase * 1.7 + dot(positionWS, float3(0.23, 0.11, 0.19));
            float2 ripple = _TreeRipple * float2(sin(ripplePhase), cos(ripplePhase * 0.8)) * (1 + _WindGust);

            float gust = _TreeGustLean * _WindGust;

            positionWS.xz += (WindDirection * (sway + gust) + ripple) * reach;

            // A small quiver per vertex, faster in a gust.
            float quiver = sin(_Time.y * (4 + 4 * _WindGust) + dot(positionWS, float3(1.7, 2.3, 1.3)));
            positionWS += float3(0.6, 0.3, 0.7) * (_TreeFlutter * (1 + _WindGust) * quiver);

            return TransformWorldToObject(positionWS);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex TreeWindVertex
            #pragma fragment LitPassFragment

            // Universal Pipeline keywords, as in URP's Lit.shader.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // Unity defined keywords.
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

            Varyings TreeWindVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz, input.texcoord);
                return LitPassVertex(input);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex TreeWindShadowVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            Varyings TreeWindShadowVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz, input.texcoord);
                return ShadowPassVertex(input);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex TreeWindDepthVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            Varyings TreeWindDepthVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                input.position.xyz = TreeWindPositionOS(input.position.xyz, input.texcoord);
                return DepthOnlyVertex(input);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex TreeWindDepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"

            Varyings TreeWindDepthNormalsVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz, input.texcoord);
                return DepthNormalsVertex(input);
            }
            ENDHLSL
        }
    }
}
