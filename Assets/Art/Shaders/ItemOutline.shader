// Thin outline around something the player can gather. It draws the back faces of a
// copy of the model, pushed outwards on screen by a fixed number of pixels: only the
// rim shows past the model, and anything in front of the model still hides it.
//
// The push follows smoothed normals stored in TEXCOORD3 (see ItemOutlineFactory): the
// hard normals of a low poly model would split the rim at every edge.
//
// Only what lies outside the model's own silhouette is drawn. On a hollowed model (the
// holes of the Champonge's cap), the pushed back faces of a hole lie just under the surface
// and would show through it as thin lines. So, for each object, URP draws the passes in
// the order of its forward tags: the mask marks the model's silhouette in the stencil
// (SRPDefaultUnlit), the outline skips what is marked (UniversalForward), and the clear
// unmarks it (UniversalForwardOnly), so the next outlined object is not hidden by this one.
Shader "Fungiiiii/ItemOutline"
{
    Properties
    {
        _Color ("Color", Color) = (0.95, 0.9, 0.8, 1)
        _Width ("Width (pixels at 1080p)", Range(0, 8)) = 2
        _FadeStart ("Full strength up to (m)", Float) = 12
        _FadeEnd ("Invisible from (m)", Float) = 25
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent-20"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Width;
            float _FadeStart;
            float _FadeEnd;
        CBUFFER_END

        // The model's silhouette, where it is drawn, for the mask and the clear.
        float4 SilhouetteVert(float4 positionOS : POSITION) : SV_POSITION
        {
            return TransformObjectToHClip(positionOS.xyz);
        }

        half4 SilhouetteFrag() : SV_Target
        {
            return 0;
        }
        ENDHLSL

        Pass
        {
            Name "SilhouetteMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest Always
            ColorMask 0
            Stencil
            {
                Ref 128
                WriteMask 128
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex SilhouetteVert
            #pragma fragment SilhouetteFrag
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil
            {
                Ref 128
                ReadMask 128
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 smoothNormalOS : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half alpha : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.smoothNormalOS);
                output.positionCS = TransformWorldToHClip(positionWS);

                // The same width in pixels at any distance, scaled with the screen height.
                float3 normalCS = mul((float3x3)UNITY_MATRIX_VP, normalWS);
                float2 direction = normalCS.xy * _ScreenParams.xy;
                direction /= max(length(direction), 1e-5);
                float pixels = _Width * _ScreenParams.y / 1080.0;
                output.positionCS.xy += direction * (2.0 * pixels / _ScreenParams.xy) * output.positionCS.w;

                float3 originWS = TransformObjectToWorld(float3(0, 0, 0));
                float distanceToCamera = distance(originWS, _WorldSpaceCameraPos);
                output.alpha = saturate((_FadeEnd - distanceToCamera) / max(_FadeEnd - _FadeStart, 0.01));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(_Color.rgb, _Color.a * input.alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "SilhouetteClear"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Off
            ZWrite Off
            ZTest Always
            ColorMask 0
            Stencil
            {
                Ref 0
                WriteMask 128
                Comp Always
                Pass Zero
            }

            HLSLPROGRAM
            #pragma vertex SilhouetteVert
            #pragma fragment SilhouetteFrag
            ENDHLSL
        }
    }
}
