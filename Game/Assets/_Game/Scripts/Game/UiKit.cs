using UnityEngine;
using UnityEngine.EventSystems;
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

        public static readonly Color OutlineColor = new Color32(0x4A, 0x26, 0x0E, 255);

        private static Font _font;
        private static bool _fontIsRounded;

        // The game font (Resources/Fonts/LilitaOne-Regular.ttf, SIL Open Font License); the built-in font if it is missing.
        public static Font UiFont()
        {
            if (_font == null)
            {
                _font = Resources.Load<Font>("Fonts/LilitaOne-Regular");
                _fontIsRounded = _font != null;
                if (_font == null)
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
            // the game font is already heavy; a synthetic bold on top would only smear it
            text.fontStyle = _fontIsRounded && style == FontStyle.Bold ? FontStyle.Normal : style;
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
                image.preserveAspect = true;
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

        // A framed cream card (the sprite carries its own colours).
        public static Image NewCard(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            var rect = NewRect(name, parent);
            Place(rect, anchor, pivot ?? anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.Card();
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return image;
        }

        // A chunky button. Buttons in the main colour get white text with a dark outline; buttons with a given (lighter)
        // colour keep dark text.
        public static Button NewButton(string name, Transform parent, out Text label, int fontSize, Color? color = null)
        {
            var rect = NewRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.Button();
            image.type = Image.Type.Sliced;
            image.color = color ?? ButtonColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.62f, 0.62f, 0.62f, 0.8f);
            button.colors = colors;

            bool primary = !color.HasValue;
            label = NewText("Label", rect, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, primary ? Color.white : TextColor);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12, UiArt.ButtonLip + 6);
            label.rectTransform.offsetMax = new Vector2(-12, -6);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = fontSize;
            if (primary)
                AddOutline(label);

            rect.gameObject.AddComponent<PressEffect>().Label = label.rectTransform;
            return button;
        }

        // A dark outline around the text, for white text on a coloured background.
        public static void AddOutline(Text text, float distance = 3f)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(OutlineColor.r, OutlineColor.g, OutlineColor.b, 0.95f);
            outline.effectDistance = new Vector2(distance, -distance);
        }

        // A wooden plate (a chunky button that cannot be pressed), e.g. behind the level number.
        public static Image NewPlate(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var rect = NewRect(name, parent);
            Place(rect, anchor, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.Button();
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // A dark capsule, e.g. behind the gold counter.
        public static Image NewPill(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var rect = NewRect(name, parent);
            Place(rect, anchor, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.RoundedRect();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 24f / (size.y * 0.5f);
            image.color = color;
            image.raycastTarget = false;
            return image;
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

            // real artwork (Resources/Art/coin.png) replaces the drawn coin
            var art = ArtLibrary.Find("coin");
            if (art != null)
            {
                var picture = Shape(root, "Art", 0, 0, size, size, Color.white, sprite: art);
                Place(picture.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
                return root;
            }

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

    // Pressing a chunky button pushes its label down a little, like a real key.
    internal sealed class PressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float Sink = 7f;

        public RectTransform Label { get; set; }

        private Vector2 _min, _max;
        private bool _pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pressed || Label == null)
                return;

            _min = Label.offsetMin;
            _max = Label.offsetMax;
            Label.offsetMin = new Vector2(_min.x, _min.y - Sink);
            Label.offsetMax = new Vector2(_max.x, _max.y - Sink);
            _pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        private void OnDisable() => Release();

        private void Release()
        {
            if (!_pressed || Label == null)
                return;

            Label.offsetMin = _min;
            Label.offsetMax = _max;
            _pressed = false;
        }
    }
}
