using System.Collections;
using System.Collections.Generic;
using SweetBazaar.Core;
using UnityEngine;

namespace SweetBazaar.Game
{
    // The whole counter: lays the boxes out for the screen and plays the move animations.
    // The model (Board) changes first; this view then catches up, so it never decides anything itself.
    internal sealed class BoardView : MonoBehaviour
    {
        private const float ColumnPitch = 1.32f;
        private const float RowGap = 0.62f;
        // At most this many boxes per row: fewer columns make the boxes bigger and easier to tap.
        private const int MaxColumnsPerRow = 4;

        private readonly List<BoxView> _boxes = new List<BoxView>();

        public IReadOnlyList<BoxView> Boxes => _boxes;

        // Rebuilds everything from the model, without animation.
        public void Show(Board board)
        {
            foreach (var box in _boxes)
            {
                if (box != null)
                    Objects.Dispose(box.gameObject);
            }
            _boxes.Clear();

            for (int i = 0; i < board.Boxes.Count; i++)
                _boxes.Add(BoxView.Create(transform, i, board.Boxes[i]));

            Layout();
        }

        // Index of the box under a world position, or -1.
        public int HitTest(Vector2 worldPoint)
        {
            for (int i = 0; i < _boxes.Count; i++)
            {
                if (_boxes[i].WorldHitRect().Contains(worldPoint))
                    return i;
            }
            return -1;
        }

        public void SetLifted(int boxIndex, bool lifted)
        {
            if (boxIndex >= 0 && boxIndex < _boxes.Count)
                _boxes[boxIndex].SetLifted(lifted);
        }

        public void ShakeBox(int boxIndex)
        {
            if (boxIndex >= 0 && boxIndex < _boxes.Count && Application.isPlaying)
                StartCoroutine(_boxes[boxIndex].Shake());
        }

        public IEnumerator PlayMove(MoveOutcome outcome)
        {
            var move = outcome.Move;
            yield return Transfer(move.From, move.To, move.Count);

            if (outcome.TargetClosed)
            {
                _boxes[move.To].ShowParcel(animate: true);
                yield return new WaitForSeconds(0.5f);
            }
        }

        public IEnumerator PlayUndo(MoveOutcome outcome)
        {
            var move = outcome.Move;
            if (outcome.TargetClosed)
                yield return _boxes[move.To].HideParcelRoutine();

            yield return Transfer(move.To, move.From, move.Count);
        }

        // The model already has the new box at the end of the list.
        public IEnumerator PlayAddBox(Board board)
        {
            var view = BoxView.Create(transform, board.Boxes.Count - 1, board.Boxes[board.Boxes.Count - 1]);
            _boxes.Add(view);
            Layout();

            view.transform.localScale = Vector3.zero;
            yield return Tween.ScaleTo(view.transform, Vector3.one, 0.3f, Tween.EaseOutBack);
        }

        public IEnumerator PlayRemoveLastBox()
        {
            var view = _boxes[_boxes.Count - 1];
            yield return Tween.ScaleTo(view.transform, Vector3.zero, 0.2f);

            _boxes.RemoveAt(_boxes.Count - 1);
            Objects.Dispose(view.gameObject);
            Layout();
        }

        // Frames the boxes in the part of the screen the HUD leaves free.
        // topReserve / bottomReserve are fractions of the screen height covered by the HUD.
        public void FitCamera(Camera camera, float topReserve, float bottomReserve)
        {
            if (_boxes.Count == 0)
                return;

            Bounds bounds = ContentBounds();
            float usable = Mathf.Max(0.3f, 1f - topReserve - bottomReserve);

            float sizeForHeight = bounds.size.y / usable * 0.5f;
            float sizeForWidth = bounds.size.x / Mathf.Max(0.1f, camera.aspect) * 0.5f;
            float size = Mathf.Max(sizeForHeight, sizeForWidth, 3.5f);

            camera.orthographic = true;
            camera.orthographicSize = size;

            // The free band is centred at this fraction of the screen height (from the bottom).
            float bandCenter = bottomReserve + usable * 0.5f;
            float visibleHeight = size * 2f;
            camera.transform.position = new Vector3(
                bounds.center.x,
                bounds.center.y - (bandCenter - 0.5f) * visibleHeight,
                -10f);
        }

        public Bounds ContentBounds()
        {
            var bounds = new Bounds(_boxes[0].transform.position, Vector3.zero);
            foreach (var box in _boxes)
            {
                Vector3 p = box.transform.position;
                bounds.Encapsulate(new Vector3(p.x - CandyArt.BoxWidth * 0.5f - 0.15f, p.y - 0.05f, 0f));
                bounds.Encapsulate(new Vector3(p.x + CandyArt.BoxWidth * 0.5f + 0.15f, p.y + box.Height + BoxView.LiftHeight + 0.15f, 0f));
            }
            return bounds;
        }

        // Boxes fill rows of at most MaxColumnsPerRow, spread evenly; the first row is the top one.
        private void Layout()
        {
            int count = _boxes.Count;
            if (count == 0)
                return;

            int rows = Mathf.CeilToInt(count / (float)MaxColumnsPerRow);
            int columns = Mathf.CeilToInt(count / (float)rows);
            float boxHeight = _boxes[0].Height;

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int column = i % columns;
                int inThisRow = Mathf.Min(columns, count - row * columns);

                float x = (column - (inThisRow - 1) * 0.5f) * ColumnPitch;
                float y = (rows - 1 - row) * (boxHeight + RowGap);
                _boxes[i].transform.localPosition = new Vector3(x, y, 0f);
            }
        }

        private IEnumerator Transfer(int sourceIndex, int targetIndex, int count)
        {
            var source = _boxes[sourceIndex];
            var target = _boxes[targetIndex];

            // Take the top candies off the source, highest first.
            var flying = new List<CandyView>();
            for (int i = 0; i < count; i++)
            {
                var candy = source.Candies[source.Candies.Count - 1];
                source.Candies.RemoveAt(source.Candies.Count - 1);
                candy.Renderer.sortingOrder = SortingOrders.CandyRaised + i;
                flying.Add(candy);
            }
            source.ClearLift();

            int firstSlot = target.Candies.Count;
            var routines = new List<IEnumerator>();
            for (int i = 0; i < count; i++)
            {
                target.Candies.Add(flying[i]);
                routines.Add(Fly(flying[i], target, firstSlot + i, i * 0.07f));
            }

            yield return Tween.All(this, routines);
        }

        private static IEnumerator Fly(CandyView candy, BoxView target, int slot, float delay)
        {
            for (float time = 0f; time < delay; time += Time.deltaTime)
                yield return null;

            yield return Tween.MoveWorld(candy.transform, target.SlotWorld(slot), 0.3f, arc: 0.7f);

            candy.transform.SetParent(target.transform, true);
            candy.transform.localPosition = target.SlotLocal(slot);
            candy.Renderer.sortingOrder = SortingOrders.Candy;
        }
    }
}
