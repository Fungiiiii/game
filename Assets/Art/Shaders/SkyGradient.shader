// A three-colour gradient sky for URP. The horizon colour matches the fog, so
// distant trees melt into the sky instead of into a flat backdrop.
Shader "Fungiiiii/SkyGradient"
{
    Properties
    {
        _TopColor ("Top", Color) = (0.42, 0.55, 0.62, 1)
        _HorizonColor ("Horizon", Color) = (0.2, 0.31, 0.3, 1)
        _BottomColor ("Bottom", Color) = (0.12, 0.18, 0.16, 1)
        _HorizonSharpness ("Horizon sharpness", Range(0.5, 8)) = 2.5
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _HorizonColor;
                half4 _BottomColor;
                half _HorizonSharpness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float height = normalize(input.direction).y;
                // Above the horizon: horizon to top. Below: horizon to bottom.
                half upward = pow(saturate(height), 1.0 / _HorizonSharpness);
                half downward = pow(saturate(-height), 1.0 / _HorizonSharpness);
                half3 colour = lerp(_HorizonColor.rgb, _TopColor.rgb, upward);
                colour = lerp(colour, _BottomColor.rgb, downward);
                return half4(colour, 1);
            }
            ENDHLSL
        }
    }
}
