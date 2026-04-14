using UnityEngine;

namespace GTA5Sky
{
    public static class GTA5StarfieldTexture
    {
        private static Texture2D cachedTexture;

        public static Texture2D GetOrCreate()
        {
            if (cachedTexture != null)
            {
                return cachedTexture;
            }

            Texture2D loaded = Resources.Load<Texture2D>("StarfieldTex");
            if (loaded != null)
            {
                cachedTexture = loaded;
                return cachedTexture;
            }

            const int width = 2048;
            const int height = 1024;
            cachedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "ProceduralStarfield",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            Color32[] pixels = new Color32[width * height];
            Color32 black = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = black;
            }

            System.Random rng = new System.Random(42);
            int starCount = 4500;

            for (int s = 0; s < starCount; s++)
            {
                int x = rng.Next(width);
                int y = rng.Next(height);
                float brightness = (float)rng.NextDouble();
                brightness = brightness * brightness;

                byte r, g, b, a;
                float colorVariation = (float)rng.NextDouble();
                if (colorVariation < 0.15f)
                {
                    float temp = 0.7f + brightness * 0.3f;
                    r = (byte)(255 * temp);
                    g = (byte)(200 * temp);
                    b = (byte)(180 * temp);
                }
                else if (colorVariation < 0.25f)
                {
                    float temp = 0.7f + brightness * 0.3f;
                    r = (byte)(200 * temp);
                    g = (byte)(210 * temp);
                    b = (byte)(255 * temp);
                }
                else
                {
                    byte v = (byte)(180 + brightness * 75);
                    r = g = b = v;
                }

                a = (byte)(brightness * 255);
                pixels[y * width + x] = new Color32(r, g, b, a);

                if (brightness > 0.6f && x > 0 && x < width - 1 && y > 0 && y < height - 1)
                {
                    byte dim = (byte)(brightness * 80);
                    byte dimA = (byte)(brightness * 100);
                    Color32 glow = new Color32(dim, dim, dim, dimA);
                    pixels[y * width + (x - 1)] = glow;
                    pixels[y * width + (x + 1)] = glow;
                    pixels[(y - 1) * width + x] = glow;
                    pixels[(y + 1) * width + x] = glow;
                }
            }

            cachedTexture.SetPixels32(pixels);
            cachedTexture.Apply(false, true);
            return cachedTexture;
        }
    }
}
