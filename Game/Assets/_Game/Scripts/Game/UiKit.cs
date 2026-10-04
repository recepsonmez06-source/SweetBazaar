using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // Small helpers to build UI from code (the game has no hand-made UI prefabs yet).
    internal static class UiKit
    {
        public static readonly Color TextColor = new Color32(0x5A, 0x34, 0x16, 255);
        public static readonly Color ButtonColor = new Color32(0xF0, 0xB2, 0x3E, 255);
        public static readonly Color SecondaryButtonColor = new Color32(0xD9, 0xC3, 0x9A, 255);
        public static readonly Color CardColor = new Color32(0xFF, 0xF7, 0xE6, 255);
        public static readonly Color GoldColor = new Color32(0xF2, 0xB9, 0x3B, 255);

        private static Font _font;

        public static Font UiFont()
        {
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null)
                    _font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            return _font;
        }

        public static Color Hex(int rgb, float alpha = 1f) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, alpha);

        public static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Text NewText(string name, Transform parent, int size, FontStyle style, TextAnchor alignment, Color? color = null)
        {
            var rect = NewRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = UiFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color ?? TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        // A coloured shape for composing pictures (characters, the shop). x, y are the bottom-left corner measured from
        // the bottom-left of the parent; shapes are discs (round = true) or rounded rectangles; rotation in degrees.
        public static Image Shape(Transform parent, string name, float x, float y, float width, float height, Color color,
            bool round = false, float rotation = 0f, Sprite sprite = null)
        {
            var rect = NewRect(name, parent);
            Place(rect, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(x + width * 0.5f, y + height * 0.5f), new Vector2(width, height));
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);

            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else if (round)
            {
                image.sprite = UiArt.Circle();
            }
            else
            {
                image.sprite = UiArt.RoundedRect();
                image.type = Image.Type.Sliced;
                // The sprite's rounded corners are 24 px; for small shapes make them proportionally smaller.
                image.pixelsPerUnitMultiplier = Mathf.Max(1f, 24f / Mathf.Max(6f, Mathf.Min(width, height) * 0.3f));
            }
            return image;
        }

        public static Image NewCard(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            var rect = NewRect(name, parent);
            Place(rect, anchor, pivot ?? anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.RoundedRect();
            image.type = Image.Type.Sliced;
            image.color = CardColor;
            return image;
        }

        public static Button NewButton(string name, Transform parent, out Text label, int fontSize, Color? color = null)
        {
            var rect = NewRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.RoundedRect();
            image.type = Image.Type.Sliced;
            image.color = color ?? ButtonColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.75f);
            button.colors = colors;

            label = NewText("Label", rect, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12, 8);
            label.rectTransform.offsetMax = new Vector2(-12, -8);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = fontSize;
            return button;
        }

        // A small round counter in the top-right corner of a button (e.g. how many undos are left).
        public static Text NewBadge(RectTransform button, int fontSize = 40)
        {
            var disc = Shape(button, "Badge", 0, 0, 74, 74, Hex(0xC7352F), round: true);
            var rect = disc.rectTransform;
            Place(rect, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-14, -10), new Vector2(74, 74));

            var rim = Shape(rect, "Rim", 0, 0, 74, 74, Hex(0xFFFFFF, 0.9f), round: true);
            Place(rim.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(74, 74));
            rim.transform.SetAsFirstSibling();
            var inner = Shape(rect, "Inner", 0, 0, 62, 62, Hex(0xC7352F), round: true);
            Place(inner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(62, 62));

            var text = NewText("Count", rect, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            return text;
        }

        // A gold coin: a disc with an inner ring.
        public static RectTransform NewCoin(string name, Transform parent, float size)
        {
            var root = NewRect(name, parent);
            Place(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));

            var outer = Shape(root, "Outer", 0, 0, size, size, Hex(0xB9811A), round: true);
            Place(outer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
            var face = Shape(root, "Face", 0, 0, size * 0.88f, size * 0.88f, Hex(0xF6C945), round: true);
            Place(face.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.88f, size * 0.88f));
            var ring = Shape(root, "Ring", 0, 0, size * 0.62f, size * 0.62f, Hex(0xE2A82B), round: true);
            Place(ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.62f, size * 0.62f));
            var center = Shape(root, "Center", 0, 0, size * 0.5f, size * 0.5f, Hex(0xF9D766), round: true);
            Place(center.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.5f, size * 0.5f));
            return root;
        }
    }

    // Runs coroutines for UI that is not a MonoBehaviour itself (the HUD and the pictures are plain classes).
    internal sealed class UiHost : MonoBehaviour
    {
    }
}
