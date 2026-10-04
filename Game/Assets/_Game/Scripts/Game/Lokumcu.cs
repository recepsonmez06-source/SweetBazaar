using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The candy maker ("lokumcu") who pops up when a level is won: a friendly cartoon built from simple shapes
    // (placeholder art until real artwork exists). He bounces in, waves, and says something; candies rain around him.
    internal sealed class Lokumcu
    {
        public const float Width = 360f;
        public const float Height = 430f;

        private readonly RectTransform _root;
        private readonly RectTransform _body;
        private readonly RectTransform _waveArm;
        private readonly RectTransform _bubble;
        private readonly Text _bubbleText;
        private readonly Image _artImage;       // set when real artwork (lokumcu_1.png / lokumcu_2.png) is used
        private readonly Sprite _art1;
        private readonly Sprite _art2;
        private readonly UiHost _host;
        private Coroutine _routine;
        private Vector2 _rest = Vector2.zero;

        public Lokumcu(Transform parent, UiHost host)
        {
            _host = host;
            _root = UiKit.NewRect("Lokumcu", parent);
            UiKit.Place(_root, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(Width, Height));

            // everything that bobs together
            _body = UiKit.NewRect("Body", _root);
            UiKit.Stretch(_body);
            _body.pivot = new Vector2(0.5f, 0f);

            var skin = UiKit.Hex(0xF2C9A0);
            var shirt = UiKit.Hex(0x3E7CB1);
            var shirtDark = UiKit.Hex(0x336591);
            var apron = UiKit.Hex(0xFFF8EC);
            var dark = UiKit.Hex(0x3B2A1E);

            UiKit.Shape(_body, "Shadow", 60, -6, 240, 34, UiKit.Hex(0x000000, 0.14f), round: true);

            // the arm that holds the tray
            UiKit.Shape(_body, "ArmLeft", 52, 96, 50, 128, shirtDark);
            UiKit.Shape(_body, "HandLeft", 56, 84, 44, 44, skin, round: true);

            // shirt and apron
            UiKit.Shape(_body, "Shirt", 88, 30, 184, 200, shirt);
            UiKit.Shape(_body, "Apron", 112, 30, 136, 162, apron);
            UiKit.Shape(_body, "ApronPocket", 150, 62, 60, 40, UiKit.Hex(0xE9DCC3));
            UiKit.Shape(_body, "ApronTie", 108, 178, 144, 14, UiKit.Hex(0xE9DCC3));

            // the tray of lokum held in front
            UiKit.Shape(_body, "Tray", 96, 118, 168, 24, UiKit.Hex(0x9A6530));
            UiKit.Shape(_body, "TrayRim", 90, 136, 180, 10, UiKit.Hex(0xB57A3C));
            UiKit.Shape(_body, "Candy0", 104, 140, 52, 40, Color.white, sprite: CandyArt.Candy(0));
            UiKit.Shape(_body, "Candy1", 154, 140, 52, 40, Color.white, sprite: CandyArt.Candy(1));
            UiKit.Shape(_body, "Candy2", 204, 140, 52, 40, Color.white, sprite: CandyArt.Candy(3));

            // head
            UiKit.Shape(_body, "EarLeft", 98, 252, 30, 40, skin, round: true);
            UiKit.Shape(_body, "EarRight", 232, 252, 30, 40, skin, round: true);
            UiKit.Shape(_body, "Head", 106, 214, 148, 138, skin, round: true);
            UiKit.Shape(_body, "CheekLeft", 120, 250, 30, 24, UiKit.Hex(0xF08A7A, 0.55f), round: true);
            UiKit.Shape(_body, "CheekRight", 210, 250, 30, 24, UiKit.Hex(0xF08A7A, 0.55f), round: true);
            UiKit.Shape(_body, "EyeLeft", 138, 280, 20, 24, dark, round: true);
            UiKit.Shape(_body, "EyeRight", 202, 280, 20, 24, dark, round: true);
            UiKit.Shape(_body, "EyeShineLeft", 146, 292, 8, 9, Color.white, round: true);
            UiKit.Shape(_body, "EyeShineRight", 210, 292, 8, 9, Color.white, round: true);
            UiKit.Shape(_body, "Nose", 164, 256, 32, 28, UiKit.Hex(0xE5AE85), round: true);
            UiKit.Shape(_body, "MustacheLeft", 118, 238, 66, 22, dark, rotation: 10f);
            UiKit.Shape(_body, "MustacheRight", 176, 238, 66, 22, dark, rotation: -10f);
            UiKit.Shape(_body, "Mouth", 158, 224, 44, 14, UiKit.Hex(0xA94438));

            // the red fez with a tassel
            UiKit.Shape(_body, "Fez", 124, 326, 112, 74, UiKit.Hex(0xC7352F));
            UiKit.Shape(_body, "FezBand", 120, 322, 120, 16, dark);
            UiKit.Shape(_body, "FezTop", 134, 392, 92, 12, UiKit.Hex(0xA82A25));
            UiKit.Shape(_body, "TasselCord", 226, 346, 8, 40, UiKit.Hex(0xF2B93B));
            UiKit.Shape(_body, "TasselEnd", 220, 326, 20, 24, UiKit.Hex(0xF2B93B), round: true);

            // the waving arm turns around the shoulder
            _waveArm = UiKit.NewRect("ArmRight", _body);
            UiKit.Place(_waveArm, Vector2.zero, new Vector2(0.5f, 1f), new Vector2(284, 200), new Vector2(50, 130));
            var sleeve = UiKit.NewRect("Sleeve", _waveArm);
            UiKit.Place(sleeve, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(50, 118));
            var sleeveImage = sleeve.gameObject.AddComponent<Image>();
            sleeveImage.raycastTarget = false;
            sleeveImage.sprite = UiArt.RoundedRect();
            sleeveImage.type = Image.Type.Sliced;
            sleeveImage.color = shirtDark;
            var hand = UiKit.NewRect("Hand", _waveArm);
            UiKit.Place(hand, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -108), new Vector2(46, 46));
            var handImage = hand.gameObject.AddComponent<Image>();
            handImage.raycastTarget = false;
            handImage.sprite = UiArt.Circle();
            handImage.color = skin;

            // speech bubble to the upper right
            _bubble = UiKit.NewRect("Bubble", _root);
            UiKit.Place(_bubble, new Vector2(0.5f, 0f), new Vector2(0f, 0.5f), new Vector2(150, 330), new Vector2(330, 100));
            var bubbleImage = _bubble.gameObject.AddComponent<Image>();
            bubbleImage.raycastTarget = false;
            bubbleImage.sprite = UiArt.RoundedRect();
            bubbleImage.type = Image.Type.Sliced;
            bubbleImage.color = Color.white;
            var tail = UiKit.Shape(_bubble, "Tail", 0, 0, 30, 30, Color.white, rotation: 45f);
            UiKit.Place(tail.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(2, -18), new Vector2(30, 30));
            _bubbleText = UiKit.NewText("Text", _bubble, 54, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Stretch(_bubbleText.rectTransform);
            _bubbleText.rectTransform.offsetMin = new Vector2(30, 6);
            _bubbleText.rectTransform.offsetMax = new Vector2(-12, -6);
            _bubbleText.resizeTextForBestFit = true;
            _bubbleText.resizeTextMinSize = 24;
            _bubbleText.resizeTextMaxSize = 54;

            // Real artwork replaces the drawn character: two frames (arm down / arm waving) shown in turn.
            _art1 = ArtLibrary.Find("lokumcu_1");
            _art2 = ArtLibrary.Find("lokumcu_2");
            if (_art1 != null)
            {
                foreach (Transform part in _body)
                    part.gameObject.SetActive(false);

                var artRect = UiKit.NewRect("Art", _body);
                UiKit.Stretch(artRect);
                _artImage = artRect.gameObject.AddComponent<Image>();
                _artImage.raycastTarget = false;
                _artImage.preserveAspect = true;
                _artImage.sprite = _art1;
            }
        }

        public void SetCheer(string text) => _bubbleText.text = text;

        // Where the candy maker stands (anchored position inside the parent); he jumps in from below this spot.
        public void SetRestPosition(Vector2 position)
        {
            _rest = position;
            _root.anchoredPosition = position;
        }

        // Shows the final pose at once (edit-mode previews have no animation).
        public void ShowStatic()
        {
            Stop();
            _root.anchoredPosition = _rest;
            _body.localScale = Vector3.one;
            _waveArm.localRotation = Quaternion.Euler(0f, 0f, -24f);
            if (_artImage != null)
                _artImage.sprite = _art2 != null ? _art2 : _art1;
            _bubble.localScale = Vector3.one;
        }

        // Bounces in from below, waves and says his line.
        public void Play()
        {
            if (!Application.isPlaying || _host == null)
            {
                ShowStatic();
                return;
            }

            Stop();
            _routine = _host.StartCoroutine(Animate());
        }

        public void Stop()
        {
            if (_routine != null && _host != null)
                _host.StopCoroutine(_routine);
            _routine = null;
        }

        private IEnumerator Animate()
        {
            var below = _rest + new Vector2(0f, -Height - 400f);
            _root.anchoredPosition = below;
            _bubble.localScale = Vector3.zero;

            // jump in from below
            for (float time = 0f; time < 0.6f; time += Time.unscaledDeltaTime)
            {
                float k = Tween.EaseOutBack(time / 0.6f);
                _root.anchoredPosition = Vector2.LerpUnclamped(below, _rest, k);
                yield return null;
            }
            _root.anchoredPosition = _rest;

            // the speech bubble pops up while he starts waving
            float waveTime = 0f;
            float bubbleStart = 0.1f;
            while (true)
            {
                waveTime += Time.unscaledDeltaTime;

                float bob = Mathf.Sin(waveTime * 6f) * 0.018f;
                _body.localScale = new Vector3(1f - bob * 0.5f, 1f + bob, 1f);
                _waveArm.localRotation = Quaternion.Euler(0f, 0f, -24f + Mathf.Sin(waveTime * 9f) * 26f);
                if (_artImage != null)
                    _artImage.sprite = (_art2 != null && (int)(waveTime / 0.22f) % 2 == 1) ? _art2 : _art1;

                if (waveTime > bubbleStart)
                {
                    float k = Mathf.Clamp01((waveTime - bubbleStart) / 0.35f);
                    _bubble.localScale = Vector3.one * Tween.EaseOutBack(k);
                }
                yield return null;
            }
        }

        // Candies falling over the given area (a UI rectangle); each piece removes itself when it has fallen.
        public static void Confetti(UiHost host, RectTransform area, int count)
        {
            if (!Application.isPlaying || host == null)
                return;

            for (int i = 0; i < count; i++)
                host.StartCoroutine(FallingCandy(area, i));
        }

        private static IEnumerator FallingCandy(RectTransform area, int index)
        {
            float delay = Random.value * 0.9f;
            for (float time = 0f; time < delay; time += Time.unscaledDeltaTime)
                yield return null;

            var rect = UiKit.NewRect("Confetti", area);
            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = CandyArt.Candy(index % CandyArt.TypeCount);
            float size = Random.Range(46f, 74f);
            UiKit.Place(rect, new Vector2(Random.value, 1f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(size, size * 0.75f));

            float duration = Random.Range(1.8f, 2.8f);
            float spin = Random.Range(-220f, 220f);
            float sway = Random.Range(-60f, 60f);
            float fall = area.rect.height + 200f;
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                if (rect == null)
                    yield break;
                float k = time / duration;
                rect.anchoredPosition = new Vector2(Mathf.Sin(k * 6f) * sway, 60f - fall * k * k);
                rect.localRotation = Quaternion.Euler(0f, 0f, spin * k);
                image.color = new Color(1f, 1f, 1f, k < 0.8f ? 1f : (1f - k) * 5f);
                yield return null;
            }
            if (rect != null)
                Object.Destroy(rect.gameObject);
        }
    }
}
