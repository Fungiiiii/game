using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Seeded value noise in [0, 1]. Pure arithmetic on an integer hash: the same
    /// seed gives the same values on every machine, unlike Mathf.PerlinNoise, which
    /// has no seed.
    /// </summary>
    public static class ValueNoise
    {
        /// <summary>Fractal sum of octaves, each twice the frequency and half the weight.</summary>
        public static float Fbm(Vector2 position, int octaves, int seed)
        {
            float sum = 0f;
            float weight = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            for (int octave = 0; octave < octaves; octave++)
            {
                sum += Sample(position * frequency, unchecked(seed + octave * 7919)) * amplitude;
                weight += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }

            return sum / weight;
        }

        public static float Sample(Vector2 position, int seed)
        {
            int x0 = Mathf.FloorToInt(position.x);
            int z0 = Mathf.FloorToInt(position.y);
            float tx = Mathf.SmoothStep(0f, 1f, position.x - x0);
            float tz = Mathf.SmoothStep(0f, 1f, position.y - z0);

            float bottom = Mathf.Lerp(Lattice(x0, z0, seed), Lattice(x0 + 1, z0, seed), tx);
            float top = Mathf.Lerp(Lattice(x0, z0 + 1, seed), Lattice(x0 + 1, z0 + 1, seed), tx);
            return Mathf.Lerp(bottom, top, tz);
        }

        /// <summary>Integer hash to [0, 1].</summary>
        public static float Lattice(int x, int z, int seed)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)z * 668265263u + (uint)seed * 2246822519u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFFu) / 16777215f;
            }
        }
    }
}
