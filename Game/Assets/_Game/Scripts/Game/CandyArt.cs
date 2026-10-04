using System.Collections.Generic;
using UnityEngine;

namespace SweetBazaar.Game
{
    // Placeholder art drawn with code until real artwork exists: candies, box frames and parcels.
    // Every candy type differs in colour AND pattern, so it stays readable for colour-blind players.
    internal static class CandyArt
    {
        // Number of distinct candy looks; must cover LevelCurve.MaxCandyTypes (a test checks this).
        public const int TypeCount = 10;

        // World-unit sizes. A box holds a column of candies, each in its own slot.
        public const float CandyWidth = 0.8f;
        public const float CandyHeight = 0.6f;
        public const float SlotPitch = 0.62f;
        public const float BoxPadding = 0.12f;
        public const float BoxWidth = 1.0f;

        public static float BoxHeight(int capacity) => capacity * SlotPitch + BoxPadding * 2f;

        private const int CandyPixelsWide = 128;
        private const int CandyPixelsHigh = 96;
        private const float FramePixelsPerUnit = 200f;

        // Base colour, then the colour of the pattern drawn on top.
        private static readonly Color[] BaseColors =
        {
            Hex(0xF29BB5), // 0 rose: pink
            Hex(0x8CC152), // 1 pistachio: green
            Hex(0xF3EEE2), // 2 coconut: off-white
            Hex(0xC62B40), // 3 pomegranate: red
            Hex(0xF7DC4B), // 4 lemon: yellow
            Hex(0x8A5A3C), // 5 coffee: brown
            Hex(0xF29A38), // 6 orange: orange
            Hex(0x5FD0BD), // 7 mint: teal
            Hex(0x7E4FB0), // 8 blackberry: purple
            Hex(0x4C84D8), // 9 blueberry: blue
        };

        private static readonly Color[] PatternColors =
        {
            Hex(0xFFFFFF), // dots
            Hex(0x4A7A24), // chunks
            Hex(0xB5AA94), // specks
            Hex(0xF4B5BF), // seeds
            Hex(0xFFF6C0), // diagonal stripes
            Hex(0xD9B28C), // horizontal stripes
            Hex(0xFFE2B8), // ring
            Hex(0x1F8F7D), // cross
            Hex(0xC8A6E8), // checker
            Hex(0xD6E6FF), // diamond
        };

        private static readonly Dictionary<int, Sprite> Candies = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> Frames = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> Packages = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> Glosses = new Dictionary<int, Sprite>();

        public static Color BaseColor(int type) => BaseColors[Mathf.Abs(type) % TypeCount];

        // Each of these returns the real artwork (Resources/Art/candy_00.png ... box_frame.png, box_parcel.png) when it
        // exists, otherwise the code-drawn placeholder.
        public static Sprite Candy(int type)
        {
            type = Mathf.Abs(type) % TypeCount;

            var art = ArtLibrary.Find("candy_" + type.ToString("00"));
            if (art != null)
                return art;

            if (!Candies.TryGetValue(type, out var sprite) || sprite == null)
                Candies[type] = sprite = BuildCandy(type);
            return sprite;
        }

        public static Sprite BoxFrame(int capacity)
        {
            var art = ArtLibrary.Find("box_frame");
            if (art != null)
                return art;

            if (!Frames.TryGetValue(capacity, out var sprite) || sprite == null)
                Frames[capacity] = sprite = BuildFrame(capacity);
            return sprite;
        }

        // Glass-like reflections laid over the candies of a box (a light streak on the left, a faint one on the right).
        // Null when real box artwork is used, which brings its own shine.
        public static Sprite BoxGloss(int capacity)
        {
            if (ArtLibrary.Find("box_frame") != null)
                return null;

            if (!Glosses.TryGetValue(capacity, out var sprite) || sprite == null)
                Glosses[capacity] = sprite = BuildGloss(capacity);
            return sprite;
        }

        private static Sprite BuildGloss(int capacity)
        {
            int w = Mathf.RoundToInt(BoxWidth * FramePixelsPerUnit);
            int h = Mathf.RoundToInt(BoxHeight(capacity) * FramePixelsPerUnit);
            var pixels = new Color32[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h;

                    // soft ends: the streaks fade in and out along the box
                    float along = Smooth(0.06f, 0.2f, v) * (1f - Smooth(0.78f, 0.94f, v));
                    float left = (1f - Smooth(0.012f, 0.03f, Mathf.Abs(u - 0.2f))) * 0.22f;
                    float right = (1f - Smooth(0.008f, 0.02f, Mathf.Abs(u - 0.83f))) * 0.14f;
                    float alpha = (left + right) * along;

                    pixels[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }

            return ToSprite(pixels, w, h, new Vector2(0.5f, 0f), FramePixelsPerUnit);
        }

        public static Sprite Package(int capacity)
        {
            var art = ArtLibrary.Find("box_parcel");
            if (art != null)
                return art;

            if (!Packages.TryGetValue(capacity, out var sprite) || sprite == null)
                Packages[capacity] = sprite = BuildPackage(capacity);
            return sprite;
        }

        // ---- candy ----

        private static Sprite BuildCandy(int type)
        {
            int w = CandyPixelsWide, h = CandyPixelsHigh;
            var pixels = new Color32[w * h];
            Color baseColor = BaseColors[type];
            Color patternColor = PatternColors[type];
            Color outline = Shade(baseColor, 0.55f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float coverage = RoundedBox(x + 0.5f, y + 0.5f, w, h, 3f, 20f, out float depth);
                    if (coverage <= 0f)
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float u = (x + 0.5f) / w;
                    float v = (y + 0.5f) / h;

                    Color color = Color.Lerp(Shade(baseColor, 0.84f), Shade(baseColor, 1.08f), v);
                    if (depth > 6f && PatternMask(type, u, v, x, y))
                        color = Color.Lerp(color, patternColor, 0.92f);

                    // roundness: a bright rim light along the top, a darker belly along the bottom
                    float edge = 1f - Smooth(0f, 11f, depth);
                    if (v > 0.55f)
                        color = Color.Lerp(color, Color.white, 0.30f * edge);
                    else
                        color = Shade(color, 1f - 0.22f * edge);

                    // glossy highlight in the upper left, and a small hard spark inside it
                    float gx = (u - 0.30f) / 0.30f, gy = (v - 0.78f) / 0.13f;
                    color = Color.Lerp(color, Color.white, 0.50f * Mathf.Clamp01(1f - (gx * gx + gy * gy)));
                    float sx = (u - 0.22f) / 0.07f, sy = (v - 0.80f) / 0.07f;
                    color = Color.Lerp(color, Color.white, 0.85f * Mathf.Clamp01(1.4f - (sx * sx + sy * sy)));

                    color = Color.Lerp(outline, color, Smooth(2.5f, 5f, depth));
                    color.a = coverage;
                    pixels[y * w + x] = color;
                }
            }

            return ToSprite(pixels, w, h, new Vector2(0.5f, 0.5f), w / CandyWidth);
        }

        // Which pixels carry the pattern. u, v are 0..1 across the candy.
        private static bool PatternMask(int type, float u, float v, int px, int py)
        {
            switch (type)
            {
                case 0: // staggered dots
                {
                    float gx = u * 5f;
                    float gy = v * 4f + (Mathf.FloorToInt(gx) % 2 == 0 ? 0f : 0.5f);
                    float fx = gx - Mathf.Floor(gx) - 0.5f, fy = gy - Mathf.Floor(gy) - 0.5f;
                    return fx * fx + fy * fy < 0.05f;
                }
                case 1: // irregular chunks
                {
                    int cx = Mathf.FloorToInt(u * 5f), cy = Mathf.FloorToInt(v * 4f);
                    float fx = u * 5f - cx - (0.25f + 0.5f * Hash(cx, cy, 1));
                    float fy = v * 4f - cy - (0.25f + 0.5f * Hash(cx, cy, 2));
                    return fx * fx + fy * fy < 0.045f;
                }
                case 2: // fine specks
                    return Hash(px / 2, py / 2, 3) > 0.84f;
                case 3: // seeds (small ellipses)
                {
                    float gx = u * 4f, gy = v * 3f + (Mathf.FloorToInt(gx) % 2 == 0 ? 0f : 0.5f);
                    float fx = gx - Mathf.Floor(gx) - 0.5f, fy = gy - Mathf.Floor(gy) - 0.5f;
                    return (fx * fx) / 0.07f + (fy * fy) / 0.03f < 1f;
                }
                case 4: // diagonal stripes
                    return (u * 1.28f + v * 0.96f) * 4f % 1f < 0.38f;
                case 5: // horizontal stripes
                    return v * 5f % 1f < 0.32f;
                case 6: // ring
                {
                    float dx = (u - 0.5f) * 1.28f, dy = (v - 0.5f) * 0.96f;
                    return Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - 0.27f) < 0.065f;
                }
                case 7: // cross
                    return Mathf.Abs(u - 0.5f) < 0.07f || Mathf.Abs(v - 0.5f) < 0.11f;
                case 8: // checker
                    return (Mathf.FloorToInt(u * 6f) + Mathf.FloorToInt(v * 4f)) % 2 == 0;
                default: // diamond
                    return Mathf.Abs(u - 0.5f) * 1.28f + Mathf.Abs(v - 0.5f) * 0.96f * 1.15f < 0.36f;
            }
        }

        // ---- box frame and parcel ----

        private static Sprite BuildFrame(int capacity)
        {
            int w = Mathf.RoundToInt(BoxWidth * FramePixelsPerUnit);
            int h = Mathf.RoundToInt(BoxHeight(capacity) * FramePixelsPerUnit);
            var pixels = new Color32[w * h];

            Color woodLight = Hex(0xDDB67C), woodDark = Hex(0xC79A5E), border = Hex(0x7A4B24), well = Hex(0xB9864B);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float coverage = RoundedBox(x + 0.5f, y + 0.5f, w, h, 1f, 24f, out float depth);
                    if (coverage <= 0f)
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float v = (y + 0.5f) / h;
                    float grain = 0.03f * Mathf.Sin((x * 0.21f) + Hash(x / 9, 0, 7) * 6f);
                    Color color = Shade(Color.Lerp(woodDark, woodLight, v), 1f + grain);

                    // a bright lip along the top edge and a thick dark foot at the bottom make the box look solid
                    float lipLight = (1f - Smooth(4f, 12f, depth)) * Smooth(0.7f, 0.95f, v);
                    color = Color.Lerp(color, Hex(0xF3D5A2), 0.55f * lipLight);
                    if (y < 16)
                        color = Shade(color, 0.72f + 0.28f * Smooth(0f, 16f, y));

                    // the recessed inside of the box: shadow under the top rim, lighter further down
                    float inside = RoundedBox(x + 0.5f, y + 0.5f, w, h, 16f, 14f, out float insideDepth);
                    if (inside > 0f)
                    {
                        Color inner = Color.Lerp(Shade(well, 1.1f), Shade(well, 0.78f), Smooth(0.78f, 1f, v));
                        color = Color.Lerp(color, inner, inside);
                        float sideShadow = 1f - Smooth(0f, 14f, insideDepth);
                        color = Shade(color, 1f - 0.22f * sideShadow);
                        color = Color.Lerp(Shade(well, 0.55f), color, Smooth(0f, 4f, insideDepth));
                    }

                    color = Color.Lerp(border, color, Smooth(4f, 8f, depth));
                    color.a = coverage;
                    pixels[y * w + x] = color;
                }
            }

            return ToSprite(pixels, w, h, new Vector2(0.5f, 0f), FramePixelsPerUnit);
        }

        // Wrapping paper that covers a finished (closed) box.
        private static Sprite BuildPackage(int capacity)
        {
            int w = Mathf.RoundToInt(BoxWidth * FramePixelsPerUnit);
            int h = Mathf.RoundToInt(BoxHeight(capacity) * FramePixelsPerUnit);
            var pixels = new Color32[w * h];

            Color paperA = Hex(0xF4C95A), paperB = Hex(0xE3A52F), edge = Hex(0xA9741A), ribbon = Hex(0xD5434B);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float coverage = RoundedBox(x + 0.5f, y + 0.5f, w, h, 1f, 24f, out float depth);
                    if (coverage <= 0f)
                    {
                        pixels[y * w + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                    Color color = Color.Lerp(paperB, paperA, (u + v) * 0.5f);

                    // soft diagonal paper sheen
                    color = Color.Lerp(color, Color.white, 0.12f * Mathf.Clamp01(Mathf.Sin((u - v) * 9f)));

                    float vertical = Mathf.Abs(x + 0.5f - w * 0.5f);
                    float horizontal = Mathf.Abs(y + 0.5f - h * 0.5f);
                    bool onRibbon = vertical < 22f || horizontal < 22f;
                    if (onRibbon)
                    {
                        float edgeDistance = Mathf.Min(vertical < 22f ? 22f - vertical : 99f, horizontal < 22f ? 22f - horizontal : 99f);
                        color = Color.Lerp(Shade(ribbon, 0.7f), ribbon, Smooth(0f, 5f, edgeDistance));
                    }

                    color = Color.Lerp(edge, color, Smooth(4f, 8f, depth));
                    color.a = coverage;
                    pixels[y * w + x] = color;
                }
            }

            return ToSprite(pixels, w, h, new Vector2(0.5f, 0f), FramePixelsPerUnit);
        }

        // ---- helpers ----

        // Coverage (0..1, antialiased) of a rounded rectangle inset by margin; depth is the distance inside its edge.
        private static float RoundedBox(float x, float y, int width, int height, float margin, float radius, out float depth)
        {
            float halfW = width * 0.5f - margin, halfH = height * 0.5f - margin;
            float qx = Mathf.Abs(x - width * 0.5f) - (halfW - radius);
            float qy = Mathf.Abs(y - height * 0.5f) - (halfH - radius);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
            depth = -distance;
            return Mathf.Clamp01(0.5f - distance);
        }

        private static float Smooth(float from, float to, float value)
        {
            float t = Mathf.Clamp01((value - from) / (to - from));
            return t * t * (3f - 2f * t);
        }

        // Deterministic pseudo-random value in [0, 1) for a cell.
        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65536f;
            }
        }

        private static Color Shade(Color color, float factor) =>
            new Color(Mathf.Clamp01(color.r * factor), Mathf.Clamp01(color.g * factor), Mathf.Clamp01(color.b * factor), 1f);

        private static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);

        private static Sprite ToSprite(Color32[] pixels, int width, int height, Vector2 pivot, float pixelsPerUnit)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "SweetBazaar procedural",
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, pixelsPerUnit);
        }
    }
}
