using UnityEngine;

namespace SweetBazaar.Game
{
    // Placeholder UI art drawn with code until real artwork exists (Resources/Art/ui_*.png, see docs/GORSEL_ISTEK.md):
    // soft shapes, a chunky glossy button, a framed card, a wooden plank and soft shadows.
    // Most sprites are white-based and tinted through Image.color; the card and the plank carry their own colours.
    internal static class UiArt
    {
        private static Sprite _roundedRect;
        private static Sprite _circle;
        private static Sprite _button;
        private static Sprite _card;
        private static Sprite _plank;
        private static Sprite _softShadow;
        private static Sprite _gradient;
        private static Sprite _vignette;
        private static Sprite _pattern;

        // Height of the dark lower lip of the chunky button (the label must stay above it).
        public const float ButtonLip = 16f;

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

            _circle = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar circle", Vector4.zero);
            return _circle;
        }

        // 9-sliced, so it stays crisp at any size.
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
                    float distance = BoxDistance(x + 0.5f, y + 0.5f, size * 0.5f, size * 0.5f, size * 0.5f - 1f, size * 0.5f - 1f, radius);
                    float coverage = Mathf.Clamp01(0.5f - distance);

                    // lighter on top, a darker rim all around
                    float shade = Mathf.Lerp(0.86f, 1f, (y + 0.5f) / size);
                    float rim = Mathf.Clamp01((-distance - 1f) / 3f);
                    shade *= Mathf.Lerp(0.72f, 1f, rim);

                    byte c = (byte)Mathf.RoundToInt(Mathf.Clamp01(shade) * 255f);
                    pixels[y * size + x] = new Color32(c, c, c, (byte)Mathf.RoundToInt(coverage * 255f));
                }
            }

            _roundedRect = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar rounded rect", new Vector4(24, 24, 24, 24));
            return _roundedRect;
        }

        // The chunky mobile-game button: a glossy face, a bright top edge and a thick dark lower lip. White-based; tint it.
        public static Sprite Button()
        {
            if (_button != null)
                return _button;

            const int size = 128;
            const float radius = 38f;
            const float border = 40f;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;

                    float outer = BoxDistance(px, py, size * 0.5f, size * 0.5f, size * 0.5f - 1f, size * 0.5f - 1f, radius);
                    float coverage = Mathf.Clamp01(0.5f - outer);
                    if (coverage <= 0f)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    // the face is the same shape lifted by the lip
                    float faceHalfHeight = (size - ButtonLip) * 0.5f - 1f;
                    float face = BoxDistance(px, py, size * 0.5f, ButtonLip + faceHalfHeight + 1f, size * 0.5f - 1f, faceHalfHeight, radius);

                    float shade;
                    if (face > 0f)
                    {
                        // the lip: darker towards the bottom
                        shade = Mathf.Lerp(0.50f, 0.64f, Mathf.Clamp01(py / ButtonLip));
                    }
                    else
                    {
                        float fromTop = (size - py) / border;
                        float topPart = Mathf.Lerp(1.00f, 0.945f, Smooth01(fromTop));
                        float faceHeight = size - ButtonLip - border;
                        float bottomPart = Mathf.Lerp(0.945f, 0.88f, Smooth01((size - border - py) / Mathf.Max(1f, faceHeight)));
                        shade = py > size - border ? topPart : bottomPart;

                        // bevel: a bright line along the upper edge, a slightly darker one along the lower edge
                        float edge = -face;
                        if (py > size * 0.55f)
                            shade += 0.05f * (1f - Smooth(0f, 5f, edge));
                        else
                            shade -= 0.07f * (1f - Smooth(0f, 6f, edge));
                    }

                    // dark outline around the whole button
                    float rim = Smooth(1.5f, 4f, -outer);
                    shade *= Mathf.Lerp(0.55f, 1f, rim);

                    byte c = (byte)Mathf.RoundToInt(Mathf.Clamp01(shade) * 255f);
                    pixels[y * size + x] = new Color32(c, c, c, (byte)Mathf.RoundToInt(coverage * 255f));
                }
            }

            _button = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar button", new Vector4(border, border, border, border));
            return _button;
        }

        // A cream card with a wooden frame. Carries its own colours (leave Image.color white).
        public static Sprite Card()
        {
            if (_card != null)
                return _card;

            const int size = 128;
            const float radius = 40f;
            var pixels = new Color32[size * size];

            Color outline = Hex(0x4E2C12), wood = Hex(0x9A6030), woodLight = Hex(0xC38A4C);
            Color creamTop = Hex(0xFFF8E8), creamBottom = Hex(0xF3E2BE);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float outer = BoxDistance(px, py, size * 0.5f, size * 0.5f, size * 0.5f - 1f, size * 0.5f - 1f, radius);
                    float coverage = Mathf.Clamp01(0.5f - outer);
                    if (coverage <= 0f)
                    {
                        pixels[y * size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float depth = -outer;
                    Color color = Color.Lerp(creamBottom, creamTop, py / size);

                    // inner shadow just inside the frame
                    float inner = Mathf.Clamp01((depth - 14f) / 10f);
                    color = Color.Lerp(Shade(color, 0.86f), color, inner);

                    // the wooden frame, lighter on top
                    Color frame = Color.Lerp(wood, woodLight, py / size);
                    float frameMix = 1f - Smooth(11f, 14f, depth);
                    color = Color.Lerp(color, frame, frameMix);

                    color = Color.Lerp(outline, color, Smooth(1.5f, 4f, depth));
                    color.a = coverage;
                    pixels[y * size + x] = color;
                }
            }

            _card = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar card", new Vector4(44, 44, 44, 44));
            return _card;
        }

        // A wooden shelf plank (own colours); used as a world sprite with draw mode Sliced.
        public static Sprite Plank()
        {
            if (_plank != null)
                return _plank;

            const int width = 64, height = 32;
            const float radius = 9f;
            var pixels = new Color32[width * height];

            Color top = Hex(0xE2B070), mid = Hex(0xC08040), bottom = Hex(0x8C5524), outline = Hex(0x4E2C12);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float outer = BoxDistance(px, py, width * 0.5f, height * 0.5f, width * 0.5f - 0.5f, height * 0.5f - 0.5f, radius);
                    float coverage = Mathf.Clamp01(0.5f - outer);
                    if (coverage <= 0f)
                    {
                        pixels[y * width + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float v = py / height;
                    Color color = v > 0.55f ? Color.Lerp(mid, top, (v - 0.55f) / 0.45f) : Color.Lerp(bottom, mid, v / 0.55f);
                    float grain = 0.025f * Mathf.Sin(px * 0.9f + Mathf.Sin(py * 0.7f) * 2f);
                    color = Shade(color, 1f + grain);
                    color = Color.Lerp(outline, color, Smooth(1f, 3f, -outer));
                    color.a = coverage;
                    pixels[y * width + x] = color;
                }
            }

            _plank = ToSprite(pixels, width, height, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar plank", new Vector4(12, 10, 12, 10));
            return _plank;
        }

        // A blurred dark rounded rectangle for drop shadows (tint/alpha through the renderer's colour); 9-sliced.
        public static Sprite SoftShadow()
        {
            if (_softShadow != null)
                return _softShadow;

            const int size = 64;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = BoxDistance(x + 0.5f, y + 0.5f, size * 0.5f, size * 0.5f, size * 0.5f - 22f, size * 0.5f - 22f, 8f);
                    float alpha = 1f - Smooth(-8f, 20f, distance);
                    pixels[y * size + x] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            _softShadow = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar shadow", new Vector4(26, 26, 26, 26));
            return _softShadow;
        }

        // ---- screen background ----

        // Warm top-to-bottom gradient, stretched over the whole screen.
        public static Sprite Gradient()
        {
            if (_gradient != null)
                return _gradient;

            const int height = 256;
            var pixels = new Color32[4 * height];
            Color top = Hex(0xFFF1D2), middle = Hex(0xFBDDA6), bottom = Hex(0xEEB872);
            for (int y = 0; y < height; y++)
            {
                float t = y / (height - 1f);
                Color color = t > 0.5f ? Color.Lerp(middle, top, (t - 0.5f) * 2f) : Color.Lerp(bottom, middle, t * 2f);
                for (int x = 0; x < 4; x++)
                    pixels[y * 4 + x] = color;
            }

            _gradient = ToSprite(pixels, 4, height, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar gradient", Vector4.zero);
            return _gradient;
        }

        // Dark corners that fade to nothing in the middle, stretched over the screen.
        public static Sprite Vignette()
        {
            if (_vignette != null)
                return _vignette;

            const int size = 128;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) * 0.6f + Mathf.Sqrt(dx * dx + dy * dy) * 0.4f;
                    float alpha = Smooth(0.55f, 1.25f, d) * 0.5f;
                    pixels[y * size + x] = new Color32(0x4A, 0x24, 0x0C, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            _vignette = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar vignette", Vector4.zero);
            return _vignette;
        }

        // A seamless oriental lattice tile (diamond lines with a small rosette in every cell); very light, for tiling.
        public static Sprite Pattern()
        {
            if (_pattern != null)
                return _pattern;

            const int size = 128;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // diamond lines: x + y and x - y are multiples of 64
                    float a = Mathf.Abs(Mathf.Repeat(x + y + 32f, 64f) - 32f);
                    float b = Mathf.Abs(Mathf.Repeat(x - y + 32f, 64f) - 32f);
                    float ink = 1f - Smooth(1.2f, 2.4f, Mathf.Min(a, b));

                    // a rosette in the middle of every cell: cell centres are at (32, 0) and (0, 32), repeating every 64
                    float cx = Mathf.Repeat(x, 64f) - 32f, cy = Mathf.Repeat(y + 32f, 64f) - 32f;
                    float c2x = Mathf.Repeat(x + 32f, 64f) - 32f, c2y = Mathf.Repeat(y, 64f) - 32f;
                    float d1 = Mathf.Sqrt(cx * cx + cy * cy), d2 = Mathf.Sqrt(c2x * c2x + c2y * c2y);
                    float d = Mathf.Min(d1, d2);
                    ink = Mathf.Max(ink, 1f - Smooth(0.5f, 2f, Mathf.Abs(d - 11f)));
                    ink = Mathf.Max(ink, 1f - Smooth(2f, 4f, d));

                    pixels[y * size + x] = new Color32(0x8A, 0x4B, 0x1C, (byte)Mathf.RoundToInt(ink * 255f));
                }
            }

            _pattern = ToSprite(pixels, size, size, new Vector2(0.5f, 0.5f), 100f, "SweetBazaar pattern", Vector4.zero, tile: true);
            return _pattern;
        }

        // ---- helpers ----

        // Signed distance to a rounded rectangle centred at (cx, cy) with the given half sizes; negative inside.
        private static float BoxDistance(float x, float y, float cx, float cy, float halfWidth, float halfHeight, float radius)
        {
            float qx = Mathf.Abs(x - cx) - (halfWidth - radius);
            float qy = Mathf.Abs(y - cy) - (halfHeight - radius);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        private static float Smooth(float from, float to, float value)
        {
            float t = Mathf.Clamp01((value - from) / (to - from));
            return t * t * (3f - 2f * t);
        }

        private static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private static Color Shade(Color color, float factor) =>
            new Color(Mathf.Clamp01(color.r * factor), Mathf.Clamp01(color.g * factor), Mathf.Clamp01(color.b * factor), color.a);

        private static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);

        private static Sprite ToSprite(Color32[] pixels, int width, int height, Vector2 pivot, float pixelsPerUnit, string name, Vector4 border, bool tile = false)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = tile ? TextureWrapMode.Repeat : TextureWrapMode.Clamp,
                name = name,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
        }
    }
}
