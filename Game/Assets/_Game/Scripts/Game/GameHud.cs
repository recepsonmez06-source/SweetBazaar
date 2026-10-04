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
        private readonly Text _shopGoldText;
        private readonly Text _shopBuiltCount;
        private readonly Text _shopHint;
        private readonly Text _shopInfo;
        private readonly Button[] _placeButtons = new Button[ShopCatalog.PlaceCount];
        private readonly Text[] _placeLabels = new Text[ShopCatalog.PlaceCount];
        private readonly Image[] _placeMarks = new Image[ShopCatalog.PlaceCount];
        private readonly Image[] _placeBackgrounds = new Image[ShopCatalog.PlaceCount];
        private readonly Button[] _styleButtons = new Button[ShopCatalog.StylesPerPlace];
        private readonly Text[] _styleLabels = new Text[ShopCatalog.StylesPerPlace];
        private readonly Image[] _styleBackgrounds = new Image[ShopCatalog.StylesPerPlace];
        private readonly Button _shopActionButton;
        private readonly Text _shopActionLabel;
        private readonly Button _closeShopButton;
        private readonly Text _closeShopLabel;

        private int _level = 1;
        private int _moves;
        private int _gold;
        private bool _winIsLastLevel;
        private int _winGoldEarned;
        private int _winGoldShown;
        private Shop _shopShown;
        private ShopPlace _selectedPlace = ShopPlace.Counter;
        private int _previewStyle;
        private bool _justBuilt;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action AddBoxClicked;
        public event Action NextClicked;
        public event Action LanguageClicked;
        public event Action ShopClicked;
        public event Action ShopClosed;

        // The player pressed "Build" or "Use this style" for the selected place and style.
        public event Action<ShopPlace, int> ShopActionClicked;

        public bool WinVisible => _winPanel.activeSelf;
        public bool StuckVisible => _stuckPanel.activeSelf;
        public bool ShopVisible => _shopPanel.activeSelf;

        // Any full-screen panel that should block the board.
        public bool ModalVisible => WinVisible || ShopVisible;

        public bool UndoInteractable => _undoButton.interactable;
        public bool AddBoxInteractable => _addBoxButton.interactable;
        public string UndoBadgeText => _undoBadge.text;
        public string AddBoxBadgeText => _addBoxBadge.text;
        public bool RightsVisible => _undoBadge.rectTransform.parent.gameObject.activeSelf;
        public string GoldText => _goldText.text;

        // What the shop screen currently offers (for tests).
        public ShopPlace SelectedShopPlace => _selectedPlace;
        public int PreviewedShopStyle => _previewStyle;
        public bool ShopActionInteractable => _shopActionButton.gameObject.activeSelf && _shopActionButton.interactable;
        public string ShopActionText => _shopActionLabel.text;

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
            var centerAnchor = new Vector2(0.5f, 0.5f);

            // ---- top bar ----
            // the level number on a wooden plate
            UiKit.NewPlate("LevelPlate", _safeArea, topCenter, new Vector2(0, -24), new Vector2(400, 116), UiKit.Hex(0xB36A32));
            _levelText = UiKit.NewText("Level", _safeArea, 64, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UiKit.Place(_levelText.rectTransform, topCenter, topCenter, new Vector2(0, -34), new Vector2(380, 90));
            UiKit.AddOutline(_levelText, 4f);

            _movesText = UiKit.NewText("Moves", _safeArea, 40, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiKit.Place(_movesText.rectTransform, topCenter, topCenter, new Vector2(0, -152), new Vector2(560, 60));

            // the gold counter in a dark capsule, the coin sitting on its left end
            UiKit.NewPill("GoldPill", _safeArea, topLeft, new Vector2(24, -30), new Vector2(250, 80), new Color(0.27f, 0.14f, 0.05f, 0.9f));
            var coin = UiKit.NewCoin("Coin", _safeArea, 78);
            UiKit.Place(coin, topLeft, centerAnchor, new Vector2(60, -70), new Vector2(78, 78));
            _goldText = UiKit.NewText("Gold", _safeArea, 50, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiKit.Place(_goldText.rectTransform, topLeft, new Vector2(0f, 0.5f), new Vector2(112, -70), new Vector2(190, 70));
            _goldText.horizontalOverflow = HorizontalWrapMode.Overflow;

            _shopButton = UiKit.NewButton("Shop", _safeArea, out _shopLabel, 40);
            UiKit.Place((RectTransform)_shopButton.transform, topLeft, topLeft, new Vector2(24, -124), new Vector2(240, 100));

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

            // ---- shop screen: picture, places to pick, three styles to pick from, one action ----
            _shopPanel = UiKit.NewRect("Shop", _safeArea).gameObject;
            UiKit.Stretch((RectTransform)_shopPanel.transform);
            _shopPanel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var shopCard = UiKit.NewCard("Card", _shopPanel.transform, centerAnchor, Vector2.zero, new Vector2(960, 1640)).rectTransform;
            _shopTitle = UiKit.NewText("Title", shopCard, 60, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiKit.Place(_shopTitle.rectTransform, topCenter, topCenter, new Vector2(0, -30), new Vector2(880, 84));

            var frame = UiKit.Shape(shopCard, "PictureFrame", 0, 0, 900, 600, UiKit.Hex(0x7A4B24));
            UiKit.Place(frame.rectTransform, topCenter, topCenter, new Vector2(0, -120), new Vector2(900, 600));
            var pictureHolder = UiKit.NewRect("Picture", shopCard);
            UiKit.Place(pictureHolder, topCenter, topCenter, new Vector2(0, -134), new Vector2(880, 572));
            pictureHolder.gameObject.AddComponent<RectMask2D>();
            _shopPicture = UiKit.NewRect("Scene", pictureHolder);
            UiKit.Place(_shopPicture, centerAnchor, centerAnchor, Vector2.zero, new Vector2(ShopPicture.Width, ShopPicture.Height));
            _shopPicture.localScale = Vector3.one * 1.1f;

            var shopCoin = UiKit.NewCoin("ShopCoin", shopCard, 52);
            UiKit.Place(shopCoin, topLeft, centerAnchor, new Vector2(70, -775), new Vector2(52, 52));
            _shopGoldText = UiKit.NewText("ShopGold", shopCard, 44, FontStyle.Bold, TextAnchor.MiddleLeft);
            UiKit.Place(_shopGoldText.rectTransform, topLeft, new Vector2(0f, 0.5f), new Vector2(104, -775), new Vector2(380, 60));
            _shopGoldText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _shopBuiltCount = UiKit.NewText("BuiltCount", shopCard, 34, FontStyle.Normal, TextAnchor.MiddleRight);
            UiKit.Place(_shopBuiltCount.rectTransform, topRight, new Vector2(1f, 0.5f), new Vector2(-50, -775), new Vector2(440, 60));

            // the six places
            for (int i = 0; i < ShopCatalog.PlaceCount; i++)
            {
                int index = i;
                var button = UiKit.NewButton("Place" + i, shopCard, out _placeLabels[i], 26, UiKit.SecondaryButtonColor);
                float x = (i - (ShopCatalog.PlaceCount - 1) * 0.5f) * 150f;
                UiKit.Place((RectTransform)button.transform, topCenter, topCenter, new Vector2(x, -830), new Vector2(142, 104));
                _placeButtons[i] = button;
                _placeBackgrounds[i] = button.GetComponent<Image>();
                _placeLabels[i].verticalOverflow = VerticalWrapMode.Truncate;
                _placeLabels[i].rectTransform.offsetMin = new Vector2(6, UiArt.ButtonLip + 4);
                _placeLabels[i].rectTransform.offsetMax = new Vector2(-6, -6);

                // a small disc in the corner: green when the place is built, grey when it still has to be built
                _placeMarks[i] = UiKit.Shape(button.transform, "Mark", 0, 0, 30, 30, UiKit.Hex(0x4C9A3C), round: true);
                UiKit.Place(_placeMarks[i].rectTransform, new Vector2(1f, 1f), centerAnchor, new Vector2(-10, -10), new Vector2(30, 30));

                button.onClick.AddListener(() => SelectPlace((ShopPlace)index));
            }

            _shopHint = UiKit.NewText("Hint", shopCard, 30, FontStyle.Italic, TextAnchor.MiddleCenter);
            UiKit.Place(_shopHint.rectTransform, topCenter, topCenter, new Vector2(0, -945), new Vector2(880, 44));

            // the three styles of the selected place
            for (int i = 0; i < ShopCatalog.StylesPerPlace; i++)
            {
                int index = i;
                var button = UiKit.NewButton("Style" + i, shopCard, out _styleLabels[i], 36, UiKit.CardColor);
                float x = (i - (ShopCatalog.StylesPerPlace - 1) * 0.5f) * 300f;
                UiKit.Place((RectTransform)button.transform, topCenter, topCenter, new Vector2(x, -1000), new Vector2(280, 190));
                _styleButtons[i] = button;
                _styleBackgrounds[i] = button.GetComponent<Image>();
                button.onClick.AddListener(() => SelectStyle(index));
            }

            _shopInfo = UiKit.NewText("Info", shopCard, 32, FontStyle.Normal, TextAnchor.UpperCenter);
            UiKit.Place(_shopInfo.rectTransform, topCenter, topCenter, new Vector2(0, -1210), new Vector2(860, 96));

            _shopActionButton = UiKit.NewButton("Action", shopCard, out _shopActionLabel, 46);
            UiKit.Place((RectTransform)_shopActionButton.transform, bottomCenter, bottomCenter, new Vector2(0, 200), new Vector2(620, 124));
            _closeShopButton = UiKit.NewButton("Close", shopCard, out _closeShopLabel, 40, UiKit.SecondaryButtonColor);
            UiKit.Place((RectTransform)_closeShopButton.transform, bottomCenter, bottomCenter, new Vector2(0, 56), new Vector2(620, 104));
            _shopPanel.SetActive(false);

            _languageButton.onClick.AddListener(() => LanguageClicked?.Invoke());
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
            _addBoxButton.onClick.AddListener(() => AddBoxClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            _shopButton.onClick.AddListener(() => ShopClicked?.Invoke());
            _winShopButton.onClick.AddListener(() => ShopClicked?.Invoke());
            _shopActionButton.onClick.AddListener(() => ShopActionClicked?.Invoke(_selectedPlace, _previewStyle));
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

        // How many undos / extra boxes are left in this level. The counters are hidden when the helps are unlimited
        // (tutorial levels), so beginners do not have to count them.
        // When a help has no free uses left but the player can pay for one, its badge shows that price (gold coloured)
        // instead of 0: undoPrice / extraBoxPrice are 0 when nothing can be bought.
        public void SetRights(int undosLeft, int extraBoxesLeft, bool unlimited, int undoPrice = 0, int extraBoxPrice = 0)
        {
            ShowBadge(_undoBadge, undosLeft, undoPrice);
            ShowBadge(_addBoxBadge, extraBoxesLeft, extraBoxPrice);
            _undoBadge.rectTransform.parent.gameObject.SetActive(!unlimited);
            _addBoxBadge.rectTransform.parent.gameObject.SetActive(!unlimited);
        }

        private static void ShowBadge(Text badge, int left, int price)
        {
            bool offerPrice = left <= 0 && price > 0;
            badge.text = (offerPrice ? price : left).ToString();
            UiKit.SetBadgeStyle(badge, offerPrice);
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

        // Opens the shop screen for the given shop. The selected place is kept if the screen was open before;
        // justBuilt celebrates a freshly built place.
        public void ShowShop(Shop shop, bool justBuilt = false)
        {
            bool wasOpen = _shopPanel.activeSelf;
            _shopShown = shop;
            _justBuilt = justBuilt;
            _shopPanel.SetActive(true);

            if (!wasOpen)
                _selectedPlace = FirstUnbuiltPlaceOrCounter(shop);
            _previewStyle = shop.IsBuilt(_selectedPlace) ? shop.StyleOf(_selectedPlace) : 0;
            RefreshShop();

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

        // Picks a place to look at (what a tap on its button does).
        public void SelectPlace(ShopPlace place)
        {
            if (_shopShown == null)
                return;

            _selectedPlace = place;
            _previewStyle = _shopShown.IsBuilt(place) ? _shopShown.StyleOf(place) : 0;
            _justBuilt = false;
            RefreshShop();
        }

        // Previews a style for the selected place (what a tap on a style card does); nothing is paid or changed yet.
        public void SelectStyle(int style)
        {
            if (_shopShown == null || style < 0 || style >= ShopCatalog.StylesPerPlace)
                return;

            _previewStyle = style;
            _justBuilt = false;
            RefreshShop();
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

            RefreshShop();
        }

        // Redraws the shop screen from the shop, the selected place and the previewed style.
        private void RefreshShop()
        {
            _shopTitle.text = _localizer.Get(LocKeys.ShopTitle);
            _closeShopLabel.text = _localizer.Get(LocKeys.ShopClose);
            _shopHint.text = _localizer.Get(LocKeys.ShopHint);

            var shop = _shopShown;
            if (shop == null)
                return;

            _shopGoldText.text = _localizer.Format(LocKeys.ShopGold, shop.Gold);
            _shopBuiltCount.text = _localizer.Format(LocKeys.ShopBuiltCount, shop.BuiltCount, ShopCatalog.PlaceCount);

            // the picture shows the shop as it is, with the previewed style on the selected place
            var styles = shop.Styles;
            styles[(int)_selectedPlace] = _previewStyle;
            ShopPicture.Build(_shopPicture, styles);

            for (int i = 0; i < ShopCatalog.PlaceCount; i++)
            {
                var place = (ShopPlace)i;
                _placeLabels[i].text = _localizer.Get(LocKeys.ShopPlaceName(place));
                _placeMarks[i].color = shop.IsBuilt(place) ? UiKit.Hex(0x4C9A3C) : UiKit.Hex(0xB8A98C);
                _placeBackgrounds[i].color = place == _selectedPlace ? UiKit.ButtonColor : UiKit.SecondaryButtonColor;
                _placeButtons[i].transform.localScale = place == _selectedPlace ? Vector3.one * 1.06f : Vector3.one;
            }

            bool built = shop.IsBuilt(_selectedPlace);
            for (int i = 0; i < ShopCatalog.StylesPerPlace; i++)
            {
                _styleLabels[i].text = _localizer.Get(LocKeys.ShopStyleName(_selectedPlace, i));

                bool previewed = i == _previewStyle;
                _styleBackgrounds[i].color = previewed ? UiKit.ButtonColor : UiKit.CardColor;
                _styleButtons[i].transform.localScale = previewed ? Vector3.one * 1.05f : Vector3.one;
            }

            // what the button does, and why it may not work yet
            string info;
            if (built)
            {
                bool same = _previewStyle == shop.StyleOf(_selectedPlace);
                _shopActionLabel.text = _localizer.Get(same ? LocKeys.ShopCurrent : LocKeys.ShopUse);
                _shopActionButton.interactable = !same;
                info = _localizer.Get(LocKeys.ShopFreeChange);
            }
            else
            {
                _shopActionLabel.text = _localizer.Format(LocKeys.ShopBuild, shop.NextCost);
                _shopActionButton.interactable = shop.CanBuild(_selectedPlace);
                info = _localizer.Get(LocKeys.ShopNotBuilt);
                if (!shop.CanBuild(_selectedPlace))
                    info += "\n" + _localizer.Format(LocKeys.ShopMissing, shop.GoldMissing);
            }

            if (_justBuilt)
                info = _localizer.Get(LocKeys.ShopJustBuilt) + "\n" + (shop.IsFullyBuilt ? _localizer.Get(LocKeys.ShopMaxed) : info);
            _shopInfo.text = info;
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

        // ---- helpers and animations ----

        private static ShopPlace FirstUnbuiltPlaceOrCounter(Shop shop)
        {
            for (int i = 0; i < ShopCatalog.PlaceCount; i++)
            {
                if (!shop.IsBuilt((ShopPlace)i))
                    return (ShopPlace)i;
            }
            return ShopPlace.Counter;
        }

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
