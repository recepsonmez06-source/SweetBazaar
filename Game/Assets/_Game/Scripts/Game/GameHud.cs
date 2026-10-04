using System;
using System.Collections;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The on-screen UI, built from code: level and move counters, gold and the shop button, the action buttons with
    // their remaining-uses counters, the win panel with the candy maker, the stuck banner and the shop screen.
    // All texts come from the Localizer; call RefreshTexts after the language changes.
    public sealed class GameHud
    {
        // Fractions of the screen height the HUD covers; the board is framed in the space between.
        public const float TopReserve = 0.20f;
        public const float BottomReserve = 0.17f;

        private readonly Localizer _localizer;
        private readonly UiHost _host;
        private readonly RectTransform _safeArea;

        // top bar
        private readonly Text _levelText;
        private readonly Text _movesText;
        private readonly Text _goldText;
        private readonly Button _languageButton;
        private readonly Text _languageLabel;
        private readonly Button _shopButton;
        private readonly Text _shopLabel;

        // action buttons
        private readonly Button _undoButton;
        private readonly Button _addBoxButton;
        private readonly Button _restartButton;
        private readonly Text _undoLabel;
        private readonly Text _addBoxLabel;
        private readonly Text _restartLabel;
        private readonly Text _undoBadge;
        private readonly Text _addBoxBadge;

        // win panel
        private readonly GameObject _winPanel;
        private readonly RectTransform _winCard;
        private readonly Text _winTitle;
        private readonly Text _winMessage;
        private readonly Text _winGoldText;
        private readonly RectTransform _winCoin;
        private readonly Text _nextLabel;
        private readonly Button _nextButton;
        private readonly Text _winShopLabel;
        private readonly Button _winShopButton;
        private readonly Lokumcu _lokumcu;

        // stuck banner
        private readonly GameObject _stuckPanel;
        private readonly Text _stuckTitle;
        private readonly Text _stuckHint;

        // shop screen
        private readonly GameObject _shopPanel;
        private readonly Text _shopTitle;
        private readonly RectTransform _shopPicture;
        private readonly Text _shopStageName;
        private readonly Text _shopStageText;
        private readonly Text _shopGoldText;
        private readonly Text _shopInfo;
        private readonly Button _upgradeButton;
        private readonly Text _upgradeLabel;
        private readonly Button _closeShopButton;
        private readonly Text _closeShopLabel;
        private readonly Image[] _stageDots = new Image[ShopStages.Count];

        private int _level = 1;
        private int _moves;
        private int _gold;
        private bool _winIsLastLevel;
        private int _winGoldEarned;
        private int _winGoldShown;
        private Shop _shopShown;
        private bool _justBuilt;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action AddBoxClicked;
        public event Action NextClicked;
        public event Action LanguageClicked;
        public event Action ShopClicked;
        public event Action UpgradeClicked;
        public event Action ShopClosed;

        public bool WinVisible => _winPanel.activeSelf;
        public bool StuckVisible => _stuckPanel.activeSelf;
        public bool ShopVisible => _shopPanel.activeSelf;

        // Any full-screen panel that should block the board.
        public bool ModalVisible => WinVisible || ShopVisible;

        public bool UndoInteractable => _undoButton.interactable;
        public bool AddBoxInteractable => _addBoxButton.interactable;
        public bool UpgradeInteractable => _upgradeButton.gameObject.activeInHierarchy && _upgradeButton.interactable;
        public string UndoBadgeText => _undoBadge.text;
        public string AddBoxBadgeText => _addBoxBadge.text;
        public string GoldText => _goldText.text;

        public GameHud(Transform parent, Camera camera, Localizer localizer)
        {
            _localizer = localizer;

            var canvasObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            _host = canvasObject.AddComponent<UiHost>();

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

            _safeArea = UiKit.NewRect("SafeArea", canvasObject.transform);
            UiKit.Stretch(_safeArea);

            var topLeft = new Vector2(0f, 1f);
            var topRight = new Vector2(1f, 1f);
            var topCenter = new Vector2(0.5f, 1f);
            var bottomCenter = new Vector2(0.5f, 0f);

            // ---- top bar ----
            _levelText = UiKit.NewText("Level", _safeArea, 68, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_levelText.rectTransform, topCenter, topCenter, new Vector2(0, -50), new Vector2(560, 100));

            _movesText = UiKit.NewText("Moves", _safeArea, 40, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiKit.Place(_movesText.rectTransform, topCenter, topCenter, new Vector2(0, -150), new Vector2(560, 60));

            var coin = UiKit.NewCoin("Coin", _safeArea, 64);
            UiKit.Place(coin, topLeft, new Vector2(0.5f, 0.5f), new Vector2(60, -62), new Vector2(64, 64));
            _goldText = UiKit.NewText("Gold", _safeArea, 52, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Place(_goldText.rectTransform, topLeft, new Vector2(0f, 0.5f), new Vector2(104, -62), new Vector2(190, 70));
            _goldText.horizontalOverflow = HorizontalWrapMode.Overflow;

            _shopButton = UiKit.NewButton("Shop", _safeArea, out _shopLabel, 40);
            UiKit.Place((RectTransform)_shopButton.transform, topLeft, topLeft, new Vector2(24, -112), new Vector2(240, 96));

            _languageButton = UiKit.NewButton("Language", _safeArea, out _languageLabel, 34);
            UiKit.Place((RectTransform)_languageButton.transform, topRight, topRight, new Vector2(-24, -34), new Vector2(230, 90));

            // ---- action buttons with counters ----
            _undoButton = UiKit.NewButton("Undo", _safeArea, out _undoLabel, 42);
            UiKit.Place((RectTransform)_undoButton.transform, bottomCenter, bottomCenter, new Vector2(-340, 70), new Vector2(310, 150));
            _undoBadge = UiKit.NewBadge((RectTransform)_undoButton.transform);

            _addBoxButton = UiKit.NewButton("AddBox", _safeArea, out _addBoxLabel, 42);
            UiKit.Place((RectTransform)_addBoxButton.transform, bottomCenter, bottomCenter, new Vector2(0, 70), new Vector2(310, 150));
            _addBoxBadge = UiKit.NewBadge((RectTransform)_addBoxButton.transform);

            _restartButton = UiKit.NewButton("Restart", _safeArea, out _restartLabel, 42);
            UiKit.Place((RectTransform)_restartButton.transform, bottomCenter, bottomCenter, new Vector2(340, 70), new Vector2(310, 150));

            // ---- stuck banner: under the level counter, so the stuck board stays visible ----
            _stuckPanel = UiKit.NewCard("Stuck", _safeArea, topCenter, new Vector2(0, -250), new Vector2(960, 190)).gameObject;
            _stuckTitle = UiKit.NewText("Title", _stuckPanel.transform, 52, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_stuckTitle.rectTransform, topCenter, topCenter, new Vector2(0, -16), new Vector2(900, 70));
            _stuckHint = UiKit.NewText("Hint", _stuckPanel.transform, 34, FontStyle.Normal, TextAnchor.UpperCenter);
            UiKit.Place(_stuckHint.rectTransform, topCenter, topCenter, new Vector2(0, -90), new Vector2(900, 90));
            _stuckPanel.SetActive(false);

            // ---- win panel: dims the board; the candy maker celebrates ----
            _winPanel = UiKit.NewRect("Win", _safeArea).gameObject;
            UiKit.Stretch((RectTransform)_winPanel.transform);
            _winPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var centerAnchor = new Vector2(0.5f, 0.5f);
            _winCard = UiKit.NewCard("Card", _winPanel.transform, centerAnchor, Vector2.zero, new Vector2(900, 1180)).rectTransform;

            _winTitle = UiKit.NewText("Title", _winCard, 66, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_winTitle.rectTransform, topCenter, topCenter, new Vector2(0, -50), new Vector2(820, 100));

            // the candy maker stands left of centre so his speech bubble fits on the right
            _lokumcu = new Lokumcu(_winCard, _host);
            var lokumcuRect = (RectTransform)_winCard.Find("Lokumcu");
            UiKit.Place(lokumcuRect, topCenter, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(Lokumcu.Width, Lokumcu.Height));
            _lokumcu.SetRestPosition(new Vector2(-110, -160));

            _winCoin = UiKit.NewCoin("WinCoin", _winCard, 80);
            UiKit.Place(_winCoin, topCenter, centerAnchor, new Vector2(-150, -650), new Vector2(80, 80));
            _winGoldText = UiKit.NewText("WinGold", _winCard, 64, FontStyle.Bold, TextAnchor.MiddleLeft, new Color32(0xB9, 0x81, 0x1A, 255));
            UiKit.Place(_winGoldText.rectTransform, topCenter, new Vector2(0f, 0.5f), new Vector2(-96, -650), new Vector2(400, 90));
            _winGoldText.horizontalOverflow = HorizontalWrapMode.Overflow;

            _winMessage = UiKit.NewText("Message", _winCard, 38, FontStyle.Normal, TextAnchor.UpperCenter);
            UiKit.Place(_winMessage.rectTransform, topCenter, topCenter, new Vector2(0, -725), new Vector2(780, 90));

            _nextButton = UiKit.NewButton("Next", _winCard, out _nextLabel, 46);
            UiKit.Place((RectTransform)_nextButton.transform, bottomCenter, bottomCenter, new Vector2(0, 210), new Vector2(580, 140));
            _winShopButton = UiKit.NewButton("WinShop", _winCard, out _winShopLabel, 40, UiKit.SecondaryButtonColor);
            UiKit.Place((RectTransform)_winShopButton.transform, bottomCenter, bottomCenter, new Vector2(0, 60), new Vector2(580, 110));
            _winPanel.SetActive(false);

            // ---- shop screen ----
            _shopPanel = UiKit.NewRect("Shop", _safeArea).gameObject;
            UiKit.Stretch((RectTransform)_shopPanel.transform);
            _shopPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var shopCard = UiKit.NewCard("Card", _shopPanel.transform, centerAnchor, Vector2.zero, new Vector2(960, 1560)).rectTransform;
            _shopTitle = UiKit.NewText("Title", shopCard, 64, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_shopTitle.rectTransform, topCenter, topCenter, new Vector2(0, -40), new Vector2(880, 96));

            var frame = UiKit.Shape(shopCard, "PictureFrame", 0, 0, 900, 600, UiKit.Hex(0x7A4B24));
            UiKit.Place(frame.rectTransform, topCenter, topCenter, new Vector2(0, -150), new Vector2(900, 600));
            var pictureHolder = UiKit.NewRect("Picture", shopCard);
            UiKit.Place(pictureHolder, topCenter, topCenter, new Vector2(0, -160), new Vector2(880, 572));
            pictureHolder.gameObject.AddComponent<RectMask2D>();
            _shopPicture = UiKit.NewRect("Scene", pictureHolder);
            UiKit.Place(_shopPicture, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ShopPicture.Width, ShopPicture.Height));
            _shopPicture.localScale = Vector3.one * 1.1f;

            _shopStageName = UiKit.NewText("StageName", shopCard, 56, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_shopStageName.rectTransform, topCenter, topCenter, new Vector2(0, -770), new Vector2(880, 80));
            _shopStageText = UiKit.NewText("StageText", shopCard, 36, FontStyle.Normal, TextAnchor.UpperCenter);
            UiKit.Place(_shopStageText.rectTransform, topCenter, topCenter, new Vector2(0, -855), new Vector2(820, 110));

            for (int i = 0; i < _stageDots.Length; i++)
            {
                float x = (i - (_stageDots.Length - 1) * 0.5f) * 70f;
                _stageDots[i] = UiKit.Shape(shopCard, "Dot" + i, 0, 0, 36, 36, UiKit.Hex(0xD9C3A0), round: true);
                UiKit.Place(_stageDots[i].rectTransform, topCenter, centerAnchor, new Vector2(x, -990), new Vector2(36, 36));
            }

            var shopCoin = UiKit.NewCoin("ShopCoin", shopCard, 60);
            UiKit.Place(shopCoin, topCenter, centerAnchor, new Vector2(-150, -1070), new Vector2(60, 60));
            _shopGoldText = UiKit.NewText("ShopGold", shopCard, 50, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Place(_shopGoldText.rectTransform, topCenter, new Vector2(0f, 0.5f), new Vector2(-104, -1070), new Vector2(400, 70));
            _shopGoldText.horizontalOverflow = HorizontalWrapMode.Overflow;

            _shopInfo = UiKit.NewText("Info", shopCard, 36, FontStyle.Normal, TextAnchor.UpperCenter);
            UiKit.Place(_shopInfo.rectTransform, topCenter, topCenter, new Vector2(0, -1115), new Vector2(840, 100));

            _upgradeButton = UiKit.NewButton("Upgrade", shopCard, out _upgradeLabel, 50);
            UiKit.Place((RectTransform)_upgradeButton.transform, bottomCenter, bottomCenter, new Vector2(0, 190), new Vector2(580, 130));
            _closeShopButton = UiKit.NewButton("Close", shopCard, out _closeShopLabel, 40, UiKit.SecondaryButtonColor);
            UiKit.Place((RectTransform)_closeShopButton.transform, bottomCenter, bottomCenter, new Vector2(0, 60), new Vector2(580, 110));
            _shopPanel.SetActive(false);

            _languageButton.onClick.AddListener(() => LanguageClicked?.Invoke());
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
            _addBoxButton.onClick.AddListener(() => AddBoxClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            _shopButton.onClick.AddListener(() => ShopClicked?.Invoke());
            _winShopButton.onClick.AddListener(() => ShopClicked?.Invoke());
            _upgradeButton.onClick.AddListener(() => UpgradeClicked?.Invoke());
            _closeShopButton.onClick.AddListener(() => ShopClosed?.Invoke());

            ApplySafeArea();
            RefreshTexts();
        }

        // ---- state setters ----

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

        public void SetGold(int gold)
        {
            _gold = gold;
            RefreshTexts();
        }

        // How many undos / extra boxes are left in this level.
        public void SetRights(int undosLeft, int extraBoxesLeft)
        {
            _undoBadge.text = undosLeft.ToString();
            _addBoxBadge.text = extraBoxesLeft.ToString();
        }

        public void SetUndoInteractable(bool value) => _undoButton.interactable = value;

        public void SetAddBoxInteractable(bool value) => _addBoxButton.interactable = value;

        // ---- panels ----

        public void ShowWin(bool isLastLevel, int goldEarned)
        {
            _winIsLastLevel = isLastLevel;
            _winGoldEarned = goldEarned;
            _winGoldShown = Application.isPlaying ? 0 : goldEarned;
            _winPanel.SetActive(true);
            RefreshTexts();

            _lokumcu.Play();
            if (Application.isPlaying)
            {
                Lokumcu.Confetti(_host, _winCard, 26);
                _host.StartCoroutine(CountUpGold());
            }
        }

        public void HideWin()
        {
            _lokumcu.Stop();
            _winPanel.SetActive(false);
        }

        public void ShowStuck() => _stuckPanel.SetActive(true);

        public void HideStuck() => _stuckPanel.SetActive(false);

        // Opens the shop screen showing the given shop; justBuilt celebrates a fresh upgrade.
        public void ShowShop(Shop shop, bool justBuilt = false)
        {
            _shopShown = shop;
            _justBuilt = justBuilt;
            _shopPanel.SetActive(true);
            ShopPicture.Build(_shopPicture, shop.Stage);
            RefreshTexts();

            if (justBuilt && Application.isPlaying)
            {
                _host.StartCoroutine(PopShopPicture());
                Lokumcu.Confetti(_host, (RectTransform)_shopPanel.transform, 30);
            }
        }

        public void HideShop()
        {
            _shopPanel.SetActive(false);
            _shopShown = null;
            _justBuilt = false;
        }

        public void RefreshTexts()
        {
            _levelText.text = _localizer.Format(LocKeys.HudLevel, _level);
            _movesText.text = _localizer.Format(LocKeys.HudMoves, _moves);
            _goldText.text = _gold.ToString();
            _shopLabel.text = _localizer.Get(LocKeys.HudShop);
            _undoLabel.text = _localizer.Get(LocKeys.HudUndo);
            _addBoxLabel.text = _localizer.Get(LocKeys.HudAddBox);
            _restartLabel.text = _localizer.Get(LocKeys.HudRestart);
            _languageLabel.text = NextLanguageName();

            _winTitle.text = _localizer.Get(LocKeys.WinTitle);
            _winMessage.text = _winIsLastLevel ? _localizer.Get(LocKeys.WinAllDone) : string.Empty;
            _winGoldText.text = _localizer.Format(LocKeys.WinGold, _winGoldShown);
            _nextLabel.text = _localizer.Get(LocKeys.WinNext);
            _winShopLabel.text = _localizer.Get(LocKeys.WinShop);
            _lokumcu.SetCheer(_localizer.Get(LocKeys.WinCheer));

            _stuckTitle.text = _localizer.Get(LocKeys.StuckTitle);
            _stuckHint.text = _localizer.Get(LocKeys.StuckHint);

            RefreshShopTexts();
        }

        private void RefreshShopTexts()
        {
            _shopTitle.text = _localizer.Get(LocKeys.ShopTitle);
            _closeShopLabel.text = _localizer.Get(LocKeys.ShopClose);
            _upgradeLabel.text = _localizer.Get(LocKeys.ShopUpgrade);

            var shop = _shopShown;
            if (shop == null)
                return;

            _shopStageName.text = _localizer.Get(LocKeys.ShopStageName(shop.Stage));
            _shopStageText.text = _localizer.Get(LocKeys.ShopStageText(shop.Stage));
            _shopGoldText.text = _localizer.Format(LocKeys.ShopGold, shop.Gold);

            for (int i = 0; i < _stageDots.Length; i++)
                _stageDots[i].color = i <= shop.Stage ? UiKit.Hex(0xF2B93B) : UiKit.Hex(0xD9C3A0);

            if (shop.IsFullyBuilt)
            {
                _shopInfo.text = _localizer.Get(LocKeys.ShopMaxed);
                _upgradeButton.gameObject.SetActive(false);
            }
            else
            {
                _upgradeButton.gameObject.SetActive(true);
                _upgradeButton.interactable = shop.CanUpgrade;

                string next = _localizer.Format(LocKeys.ShopNext, _localizer.Get(LocKeys.ShopStageName(shop.Stage + 1)), shop.NextCost);
                string status = shop.CanUpgrade ? string.Empty : "\n" + _localizer.Format(LocKeys.ShopMissing, shop.GoldMissing);
                _shopInfo.text = (_justBuilt ? _localizer.Get(LocKeys.ShopBuilt) + "\n" : string.Empty) + next + status;
            }

            if (shop.IsFullyBuilt && _justBuilt)
                _shopInfo.text = _localizer.Get(LocKeys.ShopBuilt) + "\n" + _localizer.Get(LocKeys.ShopMaxed);
        }

        // Keeps the HUD clear of notches and rounded corners.
        public void ApplySafeArea()
        {
            Rect area = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0 || area.width <= 0f)
            {
                UiKit.Stretch(_safeArea);
                return;
            }

            _safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            _safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            _safeArea.offsetMin = Vector2.zero;
            _safeArea.offsetMax = Vector2.zero;
        }

        // ---- animations ----

        private IEnumerator CountUpGold()
        {
            const float duration = 0.9f;
            yield return new WaitForSecondsRealtime(0.5f);
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                _winGoldShown = Mathf.RoundToInt(_winGoldEarned * Tween.EaseOutCubic(time / duration));
                _winGoldText.text = _localizer.Format(LocKeys.WinGold, _winGoldShown);
                float pulse = 1f + 0.12f * Mathf.Sin(time * 22f);
                _winCoin.localScale = Vector3.one * pulse;
                yield return null;
            }
            _winGoldShown = _winGoldEarned;
            _winGoldText.text = _localizer.Format(LocKeys.WinGold, _winGoldShown);
            _winCoin.localScale = Vector3.one;
        }

        private IEnumerator PopShopPicture()
        {
            var scale = _shopPicture;
            for (float time = 0f; time < 0.45f; time += Time.unscaledDeltaTime)
            {
                float k = Tween.EaseOutBack(time / 0.45f);
                scale.localScale = Vector3.one * Mathf.LerpUnclamped(1.0f, 1.1f, k);
                yield return null;
            }
            scale.localScale = Vector3.one * 1.1f;
        }

        // The language button offers the language it would switch to, in that language's own name.
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
    }
}
