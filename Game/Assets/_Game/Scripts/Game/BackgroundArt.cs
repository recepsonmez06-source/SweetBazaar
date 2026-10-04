using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The picture behind the board: a warm gradient, a light oriental lattice and dark corners.
    // Resources/Art/bg_game.png (1080 x 1920) replaces all of it.
    internal static class BackgroundArt
    {
        public static void Create(Transform parent, Camera camera)
        {
            var canvasObject = new GameObject("Background", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 9f;
            canvas.sortingOrder = -50;     // behind the board (sprites sit at order 0 and above)

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var art = ArtLibrary.Find("bg_game");
            if (art != null)
            {
                Layer(canvasObject.transform, "Art", art, Color.white, Image.Type.Simple);
                return;
            }

            Layer(canvasObject.transform, "Gradient", UiArt.Gradient(), Color.white, Image.Type.Simple);
            Layer(canvasObject.transform, "Pattern", UiArt.Pattern(), new Color(1f, 1f, 1f, 0.085f), Image.Type.Tiled);
            Layer(canvasObject.transform, "Vignette", UiArt.Vignette(), Color.white, Image.Type.Simple);
        }

        private static void Layer(Transform parent, string name, Sprite sprite, Color color, Image.Type type)
        {
            var rect = UiKit.NewRect(name, parent);
            UiKit.Stretch(rect);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = type;
            image.color = color;
            image.raycastTarget = false;
        }
    }
}
