using UnityEngine;

namespace GTA5Sky
{
    /// <summary>
    /// Generates a tileable 2D noise texture at startup for cloud FBM sampling.
    /// Replaces ~24 ALU hash ops per FBM call with 3 texture samples (one per octave).
    /// </summary>
    public static class NoiseTextureGenerator
    {
        static Texture2D cachedNoise;

        public static Texture2D GetOrCreate()
        {
            if (cachedNoise != null) return cachedNoise;

            const int size = 256;
            cachedNoise = new Texture2D(size, size, TextureFormat.R8, false)
            {
                name = "ProceduralNoise256",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float value = TileableNoise(x / (float)size, y / (float)size, size);
                    byte b = (byte)(Mathf.Clamp01(value) * 255);
                    pixels[y * size + x] = new Color32(b, b, b, 255);
                }
            }

            cachedNoise.SetPixels32(pixels);
            cachedNoise.Apply(false, true);
            return cachedNoise;
        }

        // Tileable value noise matching the shader's Hash12 + bilinear interpolation
        static float TileableNoise(float u, float v, int size)
        {
            float px = u * size;
            float py = v * size;
            int ix = (int)px;
            int iy = (int)py;
            float fx = px - ix;
            float fy = py - iy;

            // Hermite smooth
            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);

            float a = Hash12(ix, iy, size);
            float b = Hash12(ix + 1, iy, size);
            float c = Hash12(ix, iy + 1, size);
            float d = Hash12(ix + 1, iy + 1, size);

            return Mathf.Lerp(Mathf.Lerp(a, b, ux), Mathf.Lerp(c, d, ux), uy);
        }

        // Must match shader Hash12 exactly for visual consistency
        static float Hash12(int x, int y, int size)
        {
            x = ((x % size) + size) % size;
            y = ((y % size) + size) % size;
            float px = x * 0.1031f;
            float py = y * 0.1031f;
            Vector3 p3 = new Vector3(Frac(px), Frac(py), Frac(px));
            float d = Vector3.Dot(p3, new Vector3(p3.y + 33.33f, p3.z + 33.33f, p3.x + 33.33f));
            p3 += new Vector3(d, d, d);
            return Frac((p3.x + p3.y) * p3.z);
        }

        static float Frac(float v) => v - Mathf.Floor(v);
    }
}
