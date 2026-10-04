using UnityEngine;

namespace SweetBazaar.Game
{
    // Placeholder UI art: a white, soft-shaded rounded rectangle that is tinted through Image.color.
    internal static class UiArt
    {
        private static Sprite _roundedRect;
        private static Sprite _circle;

        // A plain white disc (tint it with Image.color); a flat ellipse is just a stretched disc.
        public static Sprite Circle()
        {
            if (_circle != null)
                return _circle;

            const int size = 128;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f, dy = y + 0.5f - size * 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) - (size * 0.5f - 1f);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "SweetBazaar circle",
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            _circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _circle;
        }

        // 9-sliced, so it stays crisp at any button size.
        public static Sprite RoundedRect()
        {
            if (_roundedRect != null)
                return _roundedRect;

            const int size = 64;
            const float radius = 22f;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - 1f - radius);
                    float qy = Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - 1f - radius);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float coverage = Mathf.Clamp01(0.5f - distance);

                    // lighter on top, a darker rim all around
                    float shade = Mathf.Lerp(0.86f, 1f, (y + 0.5f) / size);
                    float rim = Mathf.Clamp01((-distance - 1f) / 3f);
                    shade *= Mathf.Lerp(0.72f, 1f, rim);

                    byte c = (byte)Mathf.RoundToInt(Mathf.Clamp01(shade) * 255f);
                    pixels[y * size + x] = new Color32(c, c, c, (byte)Mathf.RoundToInt(coverage * 255f));
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "SweetBazaar rounded rect",
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            _roundedRect = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
            return _roundedRect;
        }
    }
}
