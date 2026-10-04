using System.Collections.Generic;
using UnityEngine;

namespace SweetBazaar.Game
{
    // Real artwork: a PNG named <name>.png in Assets/_Game/Resources/Art/ replaces the code-drawn placeholder of that name
    // (see docs/GORSEL_ISTEK.md for the list of names and sizes). Without the file the placeholder is used, so artwork can be
    // added piece by piece.
    internal static class ArtLibrary
    {
        private static readonly Dictionary<int, Sprite> FromTextures = new Dictionary<int, Sprite>();

        // The artwork called `name`, or null if there is none.
        public static Sprite Find(string name)
        {
            var sprite = Resources.Load<Sprite>("Art/" + name);
            if (sprite != null)
                return sprite;

            // A PNG that Unity imported as a plain texture (not as a sprite): wrap it so it can be used all the same.
            var texture = Resources.Load<Texture2D>("Art/" + name);
            if (texture == null)
                return null;

            int id = texture.GetInstanceID();
            if (!FromTextures.TryGetValue(id, out sprite) || sprite == null)
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                sprite.name = name;
                FromTextures[id] = sprite;
            }
            return sprite;
        }

        // Scale that makes the sprite fit inside width x height (keeping its proportions).
        public static float FitScale(Sprite sprite, float width, float height)
        {
            Vector3 size = sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f)
                return 1f;
            return Mathf.Min(width / size.x, height / size.y);
        }

        // Scales the renderer so its sprite fits inside width x height; the sprite stays centred on the object.
        // A code-drawn placeholder already has exactly that size, so it is left as it is (scale 1).
        public static void FitInside(SpriteRenderer renderer, float width, float height)
        {
            renderer.transform.localScale = Vector3.one * FitScale(renderer.sprite, width, height);
        }

        // Like FitInside, and also moves the object so the bottom centre of the sprite sits on the object's origin
        // (the code-drawn boxes use a bottom pivot; artwork usually has a centre pivot).
        public static float FitBottomCenter(SpriteRenderer renderer, float width, float height)
        {
            float scale = FitScale(renderer.sprite, width, height);
            Bounds bounds = renderer.sprite.bounds;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, 0f);
            return scale;
        }
    }
}
