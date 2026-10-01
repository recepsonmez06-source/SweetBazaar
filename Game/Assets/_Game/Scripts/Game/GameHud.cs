using System;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The on-screen UI, built from code: level and move counters, the buttons, and the win / stuck panels.
    // All texts come from the Localizer; call RefreshTexts after the language changes.
    public sealed class GameHud
    {
        // Fractions of the screen height the HUD covers; the board is framed in the space between.
        public const float TopReserve = 0.20f;
        public const float BottomReserve = 0.17f;

        private static readonly Color TextColor = new Color32(0x5A, 0x34, 0x16, 255);
        private static readonly Color ButtonColor = new Color32(0xF0, 0xB2, 0x3E, 255);
        private static readonly Color CardColor = new Color32(0xFF, 0xF7, 0xE6, 255);

        private readonly Localizer _localizer;
        private readonly RectTransform _safeArea;
        private readonly Text _levelText;
        private readonly Text _movesText;
        private readonly Button _languageButton;
        private readonly Text _languageLabel;
        private readonly Button _undoButton;
        private readonly Button _addBoxButton;
        private readonly Button _restartButton;
        private readonly Text _undoLabel;
        private readonly Text _addBoxLabel;
        private readonly Text _restartLabel;
        private readonly GameObject _winPanel;
        private readonly Text _winTitle;
        private readonly Text _winMessage;
        private readonly Text _nextLabel;
        private readonly Button _nextButton;
        private readonly GameObject _stuckPanel;
        private readonly Text _stuckTitle;
        private readonly Text _stuckHint;

        private int _level = 1;
        private int _moves;
        private bool _winIsLastLevel;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action AddBoxClicked;
        public event Action NextClicked;
        public event Action LanguageClicked;

        public bool WinVisible => _winPanel.activeSelf;
        public bool StuckVisible => _stuckPanel.activeSelf;
        public bool UndoInteractable => _undoButton.interactable;
        public bool AddBoxInteractable => _addBoxButton.interactable;

        public GameHud(Transform parent, Camera camera, Localizer localizer)
        {
            _localizer = localizer;

            var canvasObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5f;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _safeArea = NewRect("SafeArea", canvasObject.transform);
            Stretch(_safeArea);

            _levelText = NewText("Level", _safeArea, 68, FontStyle.Bold, TextAnchor.MiddleCenter);
            Place(_levelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(640, 100));

            _movesText = NewText("Moves", _safeArea, 40, FontStyle.Normal, TextAnchor.MiddleCenter);
            Place(_movesText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(640, 60));

            _languageButton = NewButton("Language", _safeArea, out _languageLabel, 34);
            Place((RectTransform)_languageButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24, -34), new Vector2(230, 90));

            _undoButton = NewButton("Undo", _safeArea, out _undoLabel, 42);
            Place((RectTransform)_undoButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-340, 70), new Vector2(310, 150));

            _addBoxButton = NewButton("AddBox", _safeArea, out _addBoxLabel, 42);
            Place((RectTransform)_addBoxButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(310, 150));

            _restartButton = NewButton("Restart", _safeArea, out _restartLabel, 42);
            Place((RectTransform)_restartButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(340, 70), new Vector2(310, 150));

            // Stuck banner: under the level counter, so the stuck board stays visible.
            _stuckPanel = NewCard("Stuck", _safeArea, new Vector2(0.5f, 1f), new Vector2(0, -250), new Vector2(960, 190));
            _stuckTitle = NewText("Title", _stuckPanel.transform, 52, FontStyle.Bold, TextAnchor.MiddleCenter);
            Place(_stuckTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(900, 70));
            _stuckHint = NewText("Hint", _stuckPanel.transform, 34, FontStyle.Normal, TextAnchor.UpperCenter);
            Place(_stuckHint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(900, 90));
            _stuckPanel.SetActive(false);

            // Win panel: dims the board and offers the next level.
            _winPanel = NewRect("Win", _safeArea).gameObject;
            Stretch((RectTransform)_winPanel.transform);
            var dim = _winPanel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);

            var card = NewCard("Card", _winPanel.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 640));
            _winTitle = NewText("Title", card.transform, 70, FontStyle.Bold, TextAnchor.MiddleCenter);
            Place(_winTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(780, 110));
            _winMessage = NewText("Message", card.transform, 38, FontStyle.Normal, TextAnchor.UpperCenter);
            Place(_winMessage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -200), new Vector2(740, 150));
            _nextButton = NewButton("Next", card.transform, out _nextLabel, 46);
            Place((RectTransform)_nextButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(560, 150));
            _winPanel.SetActive(false);

            _languageButton.onClick.AddListener(() => LanguageClicked?.Invoke());
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
            _addBoxButton.onClick.AddListener(() => AddBoxClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _nextButton.onClick.AddListener(() => NextClicked?.Invoke());

            ApplySafeArea();
            RefreshTexts();
        }

        public void SetLevel(int level)
        {
            _level = level;
            RefreshTexts();
        }

        public void SetMoves(int moves)
        {
            _moves = moves;
            RefreshTexts();
        }

        public void SetUndoInteractable(bool value) => _undoButton.interactable = value;

        public void SetAddBoxInteractable(bool value) => _addBoxButton.interactable = value;

        public void ShowWin(bool isLastLevel)
        {
            _winIsLastLevel = isLastLevel;
            _winPanel.SetActive(true);
            RefreshTexts();
        }

        public void HideWin() => _winPanel.SetActive(false);

        public void ShowStuck() => _stuckPanel.SetActive(true);

        public void HideStuck() => _stuckPanel.SetActive(false);

        public void RefreshTexts()
        {
            _levelText.text = _localizer.Format(LocKeys.HudLevel, _level);
            _movesText.text = _localizer.Format(LocKeys.HudMoves, _moves);
            _undoLabel.text = _localizer.Get(LocKeys.HudUndo);
            _addBoxLabel.text = _localizer.Get(LocKeys.HudAddBox);
            _restartLabel.text = _localizer.Get(LocKeys.HudRestart);
            _winTitle.text = _localizer.Get(LocKeys.WinTitle);
            _winMessage.text = _winIsLastLevel ? _localizer.Get(LocKeys.WinAllDone) : string.Empty;
            _nextLabel.text = _localizer.Get(LocKeys.WinNext);
            _stuckTitle.text = _localizer.Get(LocKeys.StuckTitle);
            _stuckHint.text = _localizer.Get(LocKeys.StuckHint);
            _languageLabel.text = NextLanguageName();
        }

        // Keeps the HUD clear of notches and rounded corners.
        public void ApplySafeArea()
        {
            Rect area = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0 || area.width <= 0f)
            {
                Stretch(_safeArea);
                return;
            }

            _safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            _safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            _safeArea.offsetMin = Vector2.zero;
            _safeArea.offsetMax = Vector2.zero;
        }

        // The button offers the language it would switch to, in that language's own name.
        private string NextLanguageName()
        {
            var languages = _localizer.Languages;
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].Language == _localizer.Language)
                    return languages[(i + 1) % languages.Count].Name;
            }
            return string.Empty;
        }

        // ---- construction helpers ----

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Font _font;

        private static Font UiFont()
        {
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null)
                    _font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            return _font;
        }

        private static Text NewText(string name, Transform parent, int size, FontStyle style, TextAnchor alignment)
        {
            var rect = NewRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = UiFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject NewCard(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = NewRect(name, parent);
            Place(rect, anchor, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.RoundedRect();
            image.type = Image.Type.Sliced;
            image.color = CardColor;
            return rect.gameObject;
        }

        private static Button NewButton(string name, Transform parent, out Text label, int fontSize)
        {
            var rect = NewRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiArt.RoundedRect();
            image.type = Image.Type.Sliced;
            image.color = ButtonColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
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
    }
}
