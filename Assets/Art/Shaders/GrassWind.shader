// Palette-textured grass that leans in the wind. The lean grows with the height
// above the tuft's root, so the root stays planted. Lighting is plain Lambert plus
// ambient, to match the palette material used by the rest of the forest.
Shader "Fungiiiii/GrassWind"
{
    Properties
    {
        _BaseMap ("Palette", 2D) = "white" {}
        _WindStrength ("Lean per metre of height", Range(0, 1)) = 0.35
        _WindSpeed ("Speed", Range(0, 5)) = 1.3
        _GustSize ("Gust size (m)", Range(1, 60)) = 14
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half _WindStrength;
            half _WindSpeed;
            half _GustSize;
        CBUFFER_END

        static const float2 WindDirection = float2(0.8, 0.6);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            half3 normalWS : TEXCOORD2;
            half fogFactor : TEXCOORD3;
        };

        float3 WindPositionWS(float3 positionOS)
        {
            float3 positionWS = TransformObjectToWorld(positionOS);
            float3 rootWS = TransformObjectToWorld(float3(0, 0, 0));
            float height = max(positionWS.y - rootWS.y, 0);

            // One phase per tuft, sliding along the wind: gusts cross the meadow.
            float phase = _Time.y * _WindSpeed - dot(rootWS.xz, WindDirection) / _GustSize * 6.2832;
            float sway = 0.35 + 0.65 * sin(phase) + 0.25 * sin(phase * 2.7 + rootWS.x);

            positionWS.xz += WindDirection * (sway * _WindStrength * height);
            return positionWS;
        }

        Varyings Vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            output.positionWS = WindPositionWS(input.positionOS.xyz);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            output.fogFactor = ComputeFogFactor(output.positionCS.z);
            return output;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half3 normalWS = normalize(input.normalWS);

                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 lighting = light.color * (light.shadowAttenuation * saturate(dot(normalWS, light.direction)));
                lighting += SampleSH(normalWS);

                return half4(MixFog(albedo * lighting, input.fogFactor), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
