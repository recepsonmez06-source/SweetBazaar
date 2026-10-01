using System.Collections;
using System.Collections.Generic;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SweetBazaar.Game
{
    // Plays one level at a time: turns taps into moves on the GameSession, tells the views what to animate,
    // and decides when the level is won or the player is stuck. The rules themselves live in Core.
    public sealed class GameController : MonoBehaviour
    {
        // The extra empty box is limited to once per level for now; later it becomes a rewarded-ad reward.
        private const int ExtraBoxesPerLevel = 1;

        private LevelPack _pack;
        private Localizer _localizer;
        private BoardView _boardView;
        private GameHud _hud;
        private Camera _camera;
        private bool _persist;

        private GameSession _session;
        private int _selected = -1;
        private bool _busy;
        private int _moves;
        private int _extraBoxesUsed;
        private int _screenWidth, _screenHeight;
        private Rect _safeArea;

        public int LevelNumber { get; private set; }

        public GameSession Session => _session;

        public GameHud Hud => _hud;

        public bool IsBusy => _busy;

        public int MovesMade => _moves;

        public int SelectedBox => _selected;

        internal void Initialize(LevelPack pack, Localizer localizer, BoardView boardView, GameHud hud, Camera camera, bool persist)
        {
            _pack = pack;
            _localizer = localizer;
            _boardView = boardView;
            _hud = hud;
            _camera = camera;
            _persist = persist;

            _hud.UndoClicked += Undo;
            _hud.RestartClicked += Restart;
            _hud.AddBoxClicked += AddExtraBox;
            _hud.NextClicked += NextLevel;
            _hud.LanguageClicked += ToggleLanguage;
            _localizer.LanguageChanged += _hud.RefreshTexts;
        }

        private void OnDestroy()
        {
            if (_hud == null)
                return;

            _hud.UndoClicked -= Undo;
            _hud.RestartClicked -= Restart;
            _hud.AddBoxClicked -= AddExtraBox;
            _hud.NextClicked -= NextLevel;
            _hud.LanguageClicked -= ToggleLanguage;
            _localizer.LanguageChanged -= _hud.RefreshTexts;
        }

        // Starts a level (1-based) from its saved starting position. Numbers outside the pack wrap to level 1.
        public void LoadLevel(int number)
        {
            if (number < 1 || number > _pack.Count)
                number = 1;

            StopAllCoroutines();
            _busy = false;

            LevelNumber = number;
            if (_persist)
                GamePrefs.CurrentLevel = number;

            _session = new GameSession(Board.FromLevel(_pack.Get(number).Definition));
            _selected = -1;
            _moves = 0;
            _extraBoxesUsed = 0;

            _hud.HideWin();
            _hud.HideStuck();
            _hud.SetLevel(number);
            _boardView.Show(_session.Board);
            RefreshHud();
            FitCamera();
        }

        // Applies moves at once, without animation or selection. For previews and tests.
        public void ApplyMovesInstantly(IEnumerable<Move> moves)
        {
            foreach (var move in moves)
            {
                if (!_session.TryMove(move.From, move.To, out _))
                    throw new System.InvalidOperationException($"Move {move} is not legal here.");
                _moves++;
            }
            _selected = -1;
            _boardView.Show(_session.Board);
            RefreshHud();
            FitCamera();
        }

        // A tap on a box (or -1 for empty space). Real input and tests both come through here.
        public void TapBox(int index)
        {
            if (_busy || _hud.WinVisible)
                return;

            if (index < 0)
            {
                Deselect();
                return;
            }

            if (_selected < 0)
            {
                if (CanPickUp(index))
                    Select(index);
                return;
            }

            if (index == _selected)
            {
                Deselect();
                return;
            }

            int from = _selected;
            if (_session.TryMove(from, index, out var outcome))
            {
                _selected = -1;
                StartCoroutine(PlayMove(outcome));
                return;
            }

            // Not a legal move: switch the selection to the tapped box if it can be picked up, otherwise shake it.
            Deselect();
            if (CanPickUp(index))
                Select(index);
            else
                _boardView.ShakeBox(index);
        }

        public void Undo()
        {
            if (_busy || _hud.WinVisible || !_session.TryUndo(out var info))
                return;

            Deselect();
            _hud.HideStuck();
            StartCoroutine(PlayUndo(info));
        }

        // Allowed at any time, even mid-animation: LoadLevel drops whatever is playing.
        public void Restart() => LoadLevel(LevelNumber);

        public void AddExtraBox()
        {
            if (_busy || _hud.WinVisible || _extraBoxesUsed >= ExtraBoxesPerLevel)
                return;

            Deselect();
            _hud.HideStuck();
            _session.AddEmptyBox();
            _extraBoxesUsed++;
            StartCoroutine(PlayAddBox());
        }

        public void NextLevel()
        {
            int next = LevelNumber + 1;
            LoadLevel(next > _pack.Count ? 1 : next);
        }

        public void ToggleLanguage()
        {
            var languages = _localizer.Languages;
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].Language == _localizer.Language)
                {
                    SetLanguage(languages[(i + 1) % languages.Count].Language);
                    return;
                }
            }
        }

        // Switches the language and remembers the choice; false if the language is not available.
        public bool SetLanguage(string language)
        {
            if (!_localizer.SetLanguage(language))
                return false;

            if (_persist)
                GamePrefs.Language = language;
            return true;
        }

        private void Update()
        {
            if (_boardView == null)
                return;

            if (Screen.width != _screenWidth || Screen.height != _screenHeight || Screen.safeArea != _safeArea)
            {
                _hud.ApplySafeArea();
                FitCamera();
            }

            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 screen = pointer.position.ReadValue();
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            TapBox(_boardView.HitTest(world));
        }

        // ---- selection ----

        private bool CanPickUp(int index)
        {
            var box = _session.Board.Boxes[index];
            return !box.IsEmpty && !box.IsClosed;
        }

        private void Select(int index)
        {
            _selected = index;
            _boardView.SetLifted(index, true);
        }

        private void Deselect()
        {
            if (_selected >= 0)
                _boardView.SetLifted(_selected, false);
            _selected = -1;
        }

        // ---- animated actions ----

        private IEnumerator PlayMove(MoveOutcome outcome)
        {
            _busy = true;
            _moves++;
            _hud.HideStuck();
            RefreshHud();

            yield return _boardView.PlayMove(outcome);

            _busy = false;
            CheckEnd();
        }

        private IEnumerator PlayUndo(UndoInfo info)
        {
            _busy = true;
            if (!info.WasAddedBox)
                _moves = Mathf.Max(0, _moves - 1);
            else
                _extraBoxesUsed = Mathf.Max(0, _extraBoxesUsed - 1);
            RefreshHud();

            if (info.WasAddedBox)
                yield return _boardView.PlayRemoveLastBox();
            else
                yield return _boardView.PlayUndo(info.UndoneMove);

            FitCamera();
            _busy = false;
            RefreshHud();
        }

        private IEnumerator PlayAddBox()
        {
            _busy = true;
            RefreshHud();
            FitCamera();

            yield return _boardView.PlayAddBox(_session.Board);

            _busy = false;
            RefreshHud();
        }

        private void CheckEnd()
        {
            if (_session.Board.IsWon)
            {
                int next = LevelNumber + 1;
                bool last = next > _pack.Count;
                if (_persist)
                    GamePrefs.CurrentLevel = last ? 1 : next;
                _hud.ShowWin(last);
            }
            else if (_session.Board.IsStuck)
            {
                _hud.ShowStuck();
            }
            RefreshHud();
        }

        private void RefreshHud()
        {
            _hud.SetMoves(_moves);
            _hud.SetUndoInteractable(!_busy && _session.CanUndo);
            _hud.SetAddBoxInteractable(!_busy && _extraBoxesUsed < ExtraBoxesPerLevel);
        }

        private void FitCamera()
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            _safeArea = Screen.safeArea;
            _boardView.FitCamera(_camera, GameHud.TopReserve, GameHud.BottomReserve);
        }
    }
}
