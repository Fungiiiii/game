// Mist that hugs the ground. Drawn on a quad in front of the camera: for each
// pixel it rebuilds the world position from the depth texture, walks the view ray
// and adds up the mist found on the way. The mist is thick at ground level (read
// from the Terrain heightmap, set by the GroundMist component) and thins out
// exponentially above it, so it follows the relief and has no edge. A slow noise
// breaks it into large patches that drift with the wind, and a soft rectangle
// keeps it to one part of the map.
Shader "Fungiiiii/GroundMist"
{
    Properties
    {
        _Color ("Colour", Color) = (0.68, 0.72, 0.95, 1)
        _Falloff ("Fade above ground (m)", Range(0.3, 10)) = 1.2
        _Density ("Density per metre", Range(0, 0.2)) = 0.04
        _MaxOpacity ("Max opacity", Range(0, 1)) = 0.5
        _MaxDistance ("Max distance (m)", Range(10, 300)) = 120
        _PatchSize ("Patch size (m)", Range(5, 150)) = 60
        _Drift ("Drift speed (m/s)", Range(0, 3)) = 0.5
        _ZoneCentre ("Zone centre (X, Z)", Vector) = (0, 30, 0, 0)
        _ZoneHalfSize ("Zone half size (X, Z)", Vector) = (300, 90, 0, 0)
        _ZoneFade ("Zone fade (m)", Range(1, 200)) = 70
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-50" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "GroundMist"
            Tags { "LightMode" = "UniversalForward" }
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Falloff;
                float _Density;
                float _MaxOpacity;
                float _MaxDistance;
                float _PatchSize;
                float _Drift;
                float4 _ZoneCentre;
                float4 _ZoneHalfSize;
                float _ZoneFade;
            CBUFFER_END

            // Set by the GroundMist component.
            TEXTURE2D(_MistHeightmap);
            float4 _MistTerrainOrigin;
            float4 _MistTerrainSize;

            static const int Steps = 8;

            // Above this height over the ground the mist is too thin to see.
            static const float LayerHeights = 5.0;

            // A Terrain heightmap stores full height as 32766 / 65535.
            static const float HeightmapScale = 65535.0 / 32766.0;

            // Same direction as the grass wind.
            static const float2 WindDirection = float2(0.8, 0.6);

            float Hash(float2 cell)
            {
                float3 p = frac(cell.xyx * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
                            lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            float GroundHeight(float2 xz)
            {
                float2 uv = (xz - _MistTerrainOrigin.xz) / _MistTerrainSize.xz;
                float stored = SAMPLE_TEXTURE2D_LOD(_MistHeightmap, sampler_LinearClamp, uv, 0).r;
                return _MistTerrainOrigin.y + stored * HeightmapScale * _MistTerrainSize.y;
            }

            float MistAt(float3 position)
            {
                float2 fromZone = abs(position.xz - _ZoneCentre.xy) - _ZoneHalfSize.xy;
                float zone = 1.0 - saturate(max(fromZone.x, fromZone.y) / _ZoneFade);

                float2 p = (position.xz - WindDirection * (_Time.y * _Drift)) / _PatchSize;
                float patches = smoothstep(0.2, 0.65, Noise(p));

                float aboveGround = max(position.y - GroundHeight(position.xz), 0.0);
                return exp(-aboveGround / _Falloff) * patches * zone;
            }

            float4 Vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half4 Frag(float4 positionCS : SV_POSITION) : SV_Target
            {
                float2 uv = positionCS.xy / _ScaledScreenParams.xy;
                float depth = SampleSceneDepth(uv);
                #if !UNITY_REVERSED_Z
                    depth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, depth);
                #endif

                float3 origin = _WorldSpaceCameraPos;
                float3 ray = ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP) - origin;
                float rayLength = length(ray);
                float3 direction = ray / rayLength;
                float travelled = min(rayLength, _MaxDistance);

                // A ray that climbs leaves the mist layer early: stop there, so the
                // few steps are spent where the mist is. Taken from the ground under
                // the camera, which is close enough over the length of a ray.
                float layerTop = GroundHeight(origin.xz) + LayerHeights * _Falloff;
                if (direction.y > 0.0)
                {
                    travelled = min(travelled, max(layerTop - origin.y, 0.0) / direction.y);
                }
                if (travelled < 0.5)
                {
                    return 0;
                }

                // Steps grow with distance: the mist near the camera needs the detail.
                // Each pixel starts at a slightly different point, which hides the steps.
                float jitter = InterleavedGradientNoise(positionCS.xy, 0);
                float thickness = 0.0;
                float previous = 0.0;
                for (int i = 0; i < Steps; i++)
                {
                    float next = (i + 1.0) / Steps;
                    next = travelled * next * next;
                    float at = lerp(previous, next, jitter);
                    thickness += MistAt(origin + direction * at) * (next - previous);
                    previous = next;
                }

                float alpha = 1.0 - exp(-_Density * thickness);
                return half4(_Color.rgb, min(alpha, _MaxOpacity));
            }
            ENDHLSL
        }
    }
}
