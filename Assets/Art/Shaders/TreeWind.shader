// URP Lit, for trees and undergrowth that sway in the wind. Every pass is URP's own:
// a wrapper moves the vertex first, then hands it to the Lit vertex function unchanged,
// so the models keep exactly the look of Palette.mat and their shadows follow the motion.
//
// The motion is the simple one of stylised low poly games, not a simulation: the whole
// plant leans downwind from its foot, more and more with the height, and its crown
// drifts slowly on top of that. Both are smooth functions of where a point is, never of
// what it is made of: wood and leaves that touch move together, so a branch stays on
// its trunk and foliage on its branch. Nothing trembles.
//
// The wind itself is in Wind.hlsl: waves that travel downwind, so a blow is seen
// passing from tree to tree, with lulls and stronger spells.
Shader "Fungiiiii/TreeWind"
{
    Properties
    {
        [Header(Wind)]
        _TreeHeight ("Height of the model (m, before scale)", Range(0.1, 60)) = 20
        _TreeCrownRadius ("Radius of the crown (m, before scale)", Range(0.1, 30)) = 6
        _TreeSway ("Lean of the top in a steady wind (m)", Range(0, 2)) = 0.5
        _TreeGustLean ("Extra lean of the top in a heard gust (m)", Range(0, 2)) = 0.8
        _TreeRipple ("Drift of the crown (m)", Range(0, 1)) = 0.15
        _TreeWindSpeed ("Speed of the crown drift", Range(0, 3)) = 0.6

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
        half _TreeHeight;
        half _TreeCrownRadius;
        half _TreeSway;
        half _TreeGustLean;
        half _TreeRipple;
        half _TreeWindSpeed;

        // Call after UNITY_SETUP_INSTANCE_ID, so the matrices are the plant's own.
        float3 TreeWindPositionOS(float3 positionOS)
        {
            float3 positionWS = TransformObjectToWorld(positionOS);
            float3 rootWS = TransformObjectToWorld(float3(0, 0, 0));

            // The Terrain scales every instance: heights and radii follow.
            float3x3 objectToWorld = (float3x3)GetObjectToWorldMatrix();
            float scale = pow(abs(determinant(objectToWorld)), 1.0 / 3.0);

            // 0 at the foot, 1 at the top. Squared: the foot stays planted and the trunk
            // bends like a rod, most near its top.
            float height = saturate((positionWS.y - rootWS.y) / max(_TreeHeight * scale, 0.01));
            float bend = height * height;

            // One wind per plant, read at its foot: the whole plant leans as one. The gust
            // heard in the sound rides the waves, so it too is seen running through the trees.
            float wind = WindStrengthAt(rootWS.xz);
            float lean = (_TreeSway * wind + _TreeGustLean * _WindGust * (0.55 + 0.45 * wind)) * bend;

            // The crown drifts slowly around that lean, more at the tip of the branches.
            // The wave is long next to a clump of leaves, so facets stay crisp.
            float2 fromTrunk = positionWS.xz - rootWS.xz;
            float reach = saturate(length(fromTrunk) / max(_TreeCrownRadius * scale, 0.01));
            float driftTime = _Time.y * _TreeWindSpeed;
            float2 drift = float2(
                sin(driftTime * 1.3 + dot(positionWS, float3(0.21, 0.13, 0.17))),
                cos(driftTime * 1.1 + dot(positionWS, float3(0.16, 0.19, 0.23))));
            drift *= _TreeRipple * height * (0.35 + 0.65 * reach) * (0.6 + 0.8 * WindStrengthAt(positionWS.xz));

            positionWS.xz += WindDirection * lean + drift;

            // A rod that bends gets a little lower: without this the crown would stretch.
            positionWS.y -= lean * lean * 0.5 / max(_TreeHeight * scale, 0.01);

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
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz);
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
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz);
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
                input.position.xyz = TreeWindPositionOS(input.position.xyz);
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
                input.positionOS.xyz = TreeWindPositionOS(input.positionOS.xyz);
                return DepthNormalsVertex(input);
            }
            ENDHLSL
        }
    }
}
