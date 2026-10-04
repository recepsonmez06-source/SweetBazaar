using System.Collections;
using System.Collections.Generic;
using SweetBazaar.Core;
using UnityEngine;

namespace SweetBazaar.Game
{
    // Sorting orders: frame < candies < lifted/flying candies < parcel < parcel label.
    internal static class SortingOrders
    {
        public const int Frame = 0;
        public const int Candy = 10;
        public const int CandyRaised = 20;
        public const int Parcel = 30;
        public const int ParcelLabel = 31;
    }

    internal static class Objects
    {
        // Destroy only works in play mode; the edit-mode tools (screenshots) need DestroyImmediate.
        public static void Dispose(GameObject go)
        {
            if (go == null)
                return;

            go.SetActive(false);
            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }
    }

    internal sealed class CandyView : MonoBehaviour
    {
        public int Type { get; private set; }
        public SpriteRenderer Renderer { get; private set; }

        public static CandyView Create(Transform parent, int type)
        {
            var go = new GameObject("Candy " + type);
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<CandyView>();
            view.Type = type;
            view.Renderer = go.AddComponent<SpriteRenderer>();
            view.Renderer.sprite = CandyArt.Candy(type);
            view.Renderer.sortingOrder = SortingOrders.Candy;
            ArtLibrary.FitInside(view.Renderer, CandyArt.CandyWidth, CandyArt.CandyHeight);
            return view;
        }
    }

    // One box on the counter: its frame, the candies inside and the parcel shown once it is closed.
    internal sealed class BoxView : MonoBehaviour
    {
        public const float LiftHeight = 0.34f;

        private readonly List<CandyView> _candies = new List<CandyView>();
        private SpriteRenderer _frame;
        private SpriteRenderer _parcel;
        private SpriteRenderer _parcelLabel;
        private Vector3 _parcelRest = Vector3.one;     // the parcel's normal scale (not 1 when real artwork is used)
        private int _liftedCount;

        public int Capacity { get; private set; }

        // Bottom -> top, mirroring the Box of the model.
        public List<CandyView> Candies => _candies;

        public bool ParcelVisible => _parcel.gameObject.activeSelf;

        public SpriteRenderer Parcel => _parcel;

        public float Height => CandyArt.BoxHeight(Capacity);

        public static BoxView Create(Transform parent, int index, Box box)
        {
            var go = new GameObject("Box " + index);
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<BoxView>();
            view.Capacity = box.Capacity;

            view._frame = AddRenderer(go.transform, "Frame", CandyArt.BoxFrame(box.Capacity), SortingOrders.Frame);
            ArtLibrary.FitBottomCenter(view._frame, CandyArt.BoxWidth, view.Height);

            view._parcel = AddRenderer(go.transform, "Parcel", CandyArt.Package(box.Capacity), SortingOrders.Parcel);
            ArtLibrary.FitBottomCenter(view._parcel, CandyArt.BoxWidth, view.Height);
            view._parcelRest = view._parcel.transform.localScale;
            view._parcel.gameObject.SetActive(false);

            view._parcelLabel = AddRenderer(view._parcel.transform, "Label", null, SortingOrders.ParcelLabel);

            view.Fill(box);
            return view;
        }

        // Puts the candy-type label in the middle of the parcel, whatever size the parcel artwork has.
        private void PlaceLabel()
        {
            float parcelScale = _parcelRest.x;
            Vector3 parcelPosition = _parcel.transform.localPosition;

            _parcelLabel.transform.localPosition = new Vector3(
                -parcelPosition.x / parcelScale, (Height * 0.5f - parcelPosition.y) / parcelScale, 0f);
            _parcelLabel.transform.localScale =
                Vector3.one * (ArtLibrary.FitScale(_parcelLabel.sprite, CandyArt.CandyWidth * 0.9f, CandyArt.CandyHeight * 0.9f) / parcelScale);
        }

        // Rebuilds the candies from the model, without animation.
        public void Fill(Box box)
        {
            StopAllCoroutines();
            foreach (var candy in _candies)
            {
                if (candy != null)
                    Objects.Dispose(candy.gameObject);
            }
            _candies.Clear();
            _liftedCount = 0;
            transform.localScale = Vector3.one;

            for (int slot = 0; slot < box.Count; slot++)
            {
                var candy = CandyView.Create(transform, box.Candies[slot]);
                candy.transform.localPosition = SlotLocal(slot);
                _candies.Add(candy);
            }

            if (box.IsClosed)
                ShowParcel(animate: false);
            else
                HideParcel(animate: false);
        }

        public Vector3 SlotLocal(int slot) =>
            new Vector3(0f, CandyArt.BoxPadding + CandyArt.SlotPitch * (slot + 0.5f), 0f);

        public Vector3 SlotWorld(int slot) => transform.TransformPoint(SlotLocal(slot));

        // Number of identical candies on top (what a tap would pick up).
        public int TopRunLength()
        {
            if (_candies.Count == 0)
                return 0;

            int top = _candies[_candies.Count - 1].Type;
            int run = 0;
            for (int i = _candies.Count - 1; i >= 0 && _candies[i].Type == top; i--)
                run++;
            return run;
        }

        // Raises (selects) or lowers the top run of candies.
        public void SetLifted(bool lifted)
        {
            int count = lifted ? TopRunLength() : _liftedCount;
            _liftedCount = lifted ? count : 0;

            for (int i = 0; i < count; i++)
            {
                var candy = _candies[_candies.Count - 1 - i];
                int slot = _candies.Count - 1 - i;
                var target = SlotLocal(slot) + (lifted ? Vector3.up * LiftHeight : Vector3.zero);
                candy.Renderer.sortingOrder = lifted ? SortingOrders.CandyRaised : SortingOrders.Candy;

                if (Application.isPlaying && isActiveAndEnabled)
                    StartCoroutine(Tween.MoveLocal(candy.transform, target, 0.1f));
                else
                    candy.transform.localPosition = target;
            }
        }

        public void ClearLift() => _liftedCount = 0;

        public void ShowParcel(bool animate)
        {
            var type = _candies.Count > 0 ? _candies[0].Type : 0;
            _parcelLabel.sprite = CandyArt.Candy(type);
            _parcel.gameObject.SetActive(true);
            _parcel.transform.localScale = _parcelRest;
            PlaceLabel();

            if (animate && Application.isPlaying && isActiveAndEnabled)
            {
                _parcel.transform.localScale = new Vector3(_parcelRest.x, 0f, _parcelRest.z);
                StartCoroutine(PopIn());
            }
        }

        public void HideParcel(bool animate)
        {
            if (animate && Application.isPlaying && isActiveAndEnabled && _parcel.gameObject.activeSelf)
                StartCoroutine(PopOut());
            else
                _parcel.gameObject.SetActive(false);
        }

        public IEnumerator HideParcelRoutine()
        {
            yield return PopOut();
        }

        private IEnumerator PopIn()
        {
            yield return Tween.ScaleTo(_parcel.transform, _parcelRest, 0.32f, Tween.EaseOutBack);
            yield return Pulse();
        }

        private IEnumerator PopOut()
        {
            yield return Tween.ScaleTo(_parcel.transform, new Vector3(_parcelRest.x, 0f, _parcelRest.z), 0.16f);
            _parcel.gameObject.SetActive(false);
            _parcel.transform.localScale = _parcelRest;
        }

        private IEnumerator Pulse()
        {
            yield return Tween.ScaleTo(transform, Vector3.one * 1.08f, 0.09f);
            yield return Tween.ScaleTo(transform, Vector3.one, 0.14f, Tween.EaseOutBack);
        }

        public IEnumerator Shake()
        {
            float baseX = transform.localPosition.x;
            const float amplitude = 0.07f;
            for (float time = 0f; time < 0.24f; time += Time.deltaTime)
            {
                float k = time / 0.24f;
                var position = transform.localPosition;
                position.x = baseX + Mathf.Sin(k * Mathf.PI * 6f) * amplitude * (1f - k);
                transform.localPosition = position;
                yield return null;
            }
            var end = transform.localPosition;
            end.x = baseX;
            transform.localPosition = end;
        }

        // Click target in world space: the frame plus room above for lifted candies.
        public Rect WorldHitRect()
        {
            Vector3 origin = transform.position;
            float halfWidth = CandyArt.BoxWidth * 0.5f + 0.12f;
            return new Rect(origin.x - halfWidth, origin.y - 0.05f, halfWidth * 2f, Height + LiftHeight + 0.1f);
        }

        private static SpriteRenderer AddRenderer(Transform parent, string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
