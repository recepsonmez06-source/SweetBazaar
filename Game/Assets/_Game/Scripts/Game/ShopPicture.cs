using System.Collections.Generic;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The picture of the candy shop, layered: a background and, for every place the player has built, the style chosen for it.
    // Real artwork (Resources/Art/shop_bg.png and shop_<place>_<style>.png, each covering the whole picture) replaces the
    // code-drawn placeholder layer by layer. Places that are not built are simply missing from the picture.
    internal static class ShopPicture
    {
        public const float Width = 800f;
        public const float Height = 520f;

        private static readonly Color Wood = UiKit.Hex(0xA8723A);
        private static readonly Color WoodDark = UiKit.Hex(0x7A4B24);
        private static readonly Color Glass = UiKit.Hex(0xBFE3F2, 0.38f);
        private static readonly Color Shine = UiKit.Hex(0xFFFFFF, 0.7f);

        // Name used in the artwork file names (shop_<id>_<style>.png).
        public static string ArtId(ShopPlace place)
        {
            switch (place)
            {
                case ShopPlace.Counter: return "counter";
                case ShopPlace.Display: return "display";
                case ShopPlace.Sign: return "sign";
                case ShopPlace.TeaCorner: return "tea";
                case ShopPlace.Facade: return "facade";
                default: return "decor";
            }
        }

        // Fills `root` (Width x Height) with the picture. styles[(int)place] is the style to show, or Shop.NotBuilt.
        public static void Build(RectTransform root, IReadOnlyList<int> styles)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                Objects.Dispose(root.GetChild(i).gameObject);

            var background = ArtLibrary.Find("shop_bg");
            if (background != null)
                Overlay(root, "Background", background);
            else
                Background(root);

            int facade = styles[(int)ShopPlace.Facade];
            bool hasFacade = facade != Shop.NotBuilt;

            // back to front: facade, counter, display, sign, tea corner, decoration
            Place(root, ShopPlace.Facade, facade, Facade);
            Place(root, ShopPlace.Counter, styles[(int)ShopPlace.Counter], (r, style) => Counter(r, style));
            Place(root, ShopPlace.Display, styles[(int)ShopPlace.Display], (r, style) => Display(r, style));
            Place(root, ShopPlace.Sign, styles[(int)ShopPlace.Sign], (r, style) => Sign(r, style, raised: hasFacade));
            Place(root, ShopPlace.TeaCorner, styles[(int)ShopPlace.TeaCorner], TeaCorner);
            Place(root, ShopPlace.Decor, styles[(int)ShopPlace.Decor], (r, style) => Decor(r, style, hasFacade));
        }

        private delegate void DrawStyle(RectTransform root, int style);

        // Draws one place in its style: the real artwork if it exists, otherwise the placeholder drawing.
        private static void Place(RectTransform root, ShopPlace place, int style, DrawStyle draw)
        {
            if (style == Shop.NotBuilt)
                return;

            var art = ArtLibrary.Find("shop_" + ArtId(place) + "_" + style);
            if (art != null)
                Overlay(root, "Art " + ArtId(place), art);
            else
                draw(root, style);
        }

        private static void Overlay(RectTransform root, string name, Sprite sprite)
        {
            var image = UiKit.Shape(root, name, 0, 0, Width, Height, Color.white, sprite: sprite);
            UiKit.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, Height));
        }

        // ---- placeholder drawings ----

        private static void Background(RectTransform root)
        {
            UiKit.Shape(root, "Sky", 0, 190, Width, 330, UiKit.Hex(0xCDE9F7));
            UiKit.Shape(root, "Sun", 640, 396, 96, 96, UiKit.Hex(0xFFE9A8), round: true);

            // far away bazaar roofs and domes
            UiKit.Shape(root, "FarA", 20, 190, 140, 130, UiKit.Hex(0xD9B8A0));
            UiKit.Shape(root, "FarADome", 48, 296, 84, 84, UiKit.Hex(0xC99B83), round: true);
            UiKit.Shape(root, "FarB", 170, 190, 120, 170, UiKit.Hex(0xC9B3D6));
            UiKit.Shape(root, "FarC", 560, 190, 130, 150, UiKit.Hex(0xE2C792));
            UiKit.Shape(root, "FarCDome", 580, 310, 90, 90, UiKit.Hex(0xD1AE72), round: true);
            UiKit.Shape(root, "FarD", 690, 190, 110, 110, UiKit.Hex(0xBFD4C2));

            UiKit.Shape(root, "Ground", 0, 0, Width, 206, UiKit.Hex(0xD9C3A0));
            for (int i = 0; i < 6; i++)
                UiKit.Shape(root, "Stone", 20 + i * 135, 20 + (i % 2) * 60, 100, 12, UiKit.Hex(0xBFA77F, 0.6f));
            for (int i = 0; i < 5; i++)
                UiKit.Shape(root, "Stone2", 80 + i * 150, 120 - (i % 2) * 40, 90, 10, UiKit.Hex(0xBFA77F, 0.5f));
        }

        // The shop front behind the counter: 0 striped awning, 1 arched front, 2 stone arch with a small dome.
        private static void Facade(RectTransform root, int style)
        {
            if (style == 1)
            {
                UiKit.Shape(root, "Wall", 40, 170, 720, 250, UiKit.Hex(0xF1DDBA));
                UiKit.Shape(root, "WallBase", 40, 170, 720, 26, UiKit.Hex(0xC9A777));
                UiKit.Shape(root, "Cornice", 30, 400, 740, 26, UiKit.Hex(0x3AA6A0));
                for (int i = 0; i < 18; i++)
                    UiKit.Shape(root, "CorniceTile", 40 + i * 40, 406, 24, 14, UiKit.Hex(0xFFF8EC));

                // arched door and two arched windows
                UiKit.Shape(root, "DoorArch", 335, 262, 130, 130, WoodDark, round: true);
                UiKit.Shape(root, "Door", 335, 196, 130, 130, WoodDark);
                UiKit.Shape(root, "DoorGlass", 353, 210, 94, 150, UiKit.Hex(0xE9C27A));
                foreach (float x in new[] { 90f, 590f })
                {
                    UiKit.Shape(root, "WindowArch", x - 6, 276, 132, 90, UiKit.Hex(0x3AA6A0), round: true);
                    UiKit.Shape(root, "WindowFrame", x - 6, 230, 132, 100, UiKit.Hex(0x3AA6A0));
                    UiKit.Shape(root, "WindowArchGlass", x + 6, 284, 108, 74, UiKit.Hex(0xBFE3F2), round: true);
                    UiKit.Shape(root, "WindowGlass", x + 6, 238, 108, 92, UiKit.Hex(0xBFE3F2));
                    UiKit.Shape(root, "WindowBar", x + 56, 238, 8, 120, UiKit.Hex(0x3AA6A0));
                }
                return;
            }

            if (style == 2)
            {
                UiKit.Shape(root, "Wall", 40, 170, 720, 250, UiKit.Hex(0xD3CDC2));
                for (int row = 0; row < 6; row++)
                {
                    UiKit.Shape(root, "BrickRow", 40, 190 + row * 40, 720, 4, UiKit.Hex(0xAFA89B, 0.7f));
                    for (int i = 0; i < 9; i++)
                        UiKit.Shape(root, "Brick", 40 + i * 84 + (row % 2) * 40, 190 + row * 40, 4, 40, UiKit.Hex(0xAFA89B, 0.7f));
                }
                UiKit.Shape(root, "WallBase", 40, 170, 720, 22, UiKit.Hex(0x9E9789));

                // a big stone arch around the entrance and a small dome on the roof
                UiKit.Shape(root, "ArchOuter", 290, 230, 220, 220, UiKit.Hex(0x9E9789), round: true);
                UiKit.Shape(root, "ArchOuterBase", 290, 170, 220, 140, UiKit.Hex(0x9E9789));
                UiKit.Shape(root, "ArchInner", 312, 240, 176, 180, UiKit.Hex(0x4A3A2E), round: true);
                UiKit.Shape(root, "ArchInnerBase", 312, 196, 176, 130, UiKit.Hex(0x4A3A2E));
                UiKit.Shape(root, "ArchGlow", 340, 200, 120, 190, UiKit.Hex(0xE9C27A, 0.85f), round: true);
                UiKit.Shape(root, "Dome", 350, 396, 100, 100, UiKit.Hex(0x7FB6C4), round: true);
                UiKit.Shape(root, "DomeBase", 340, 392, 120, 22, UiKit.Hex(0x9E9789));
                UiKit.Shape(root, "Finial", 396, 480, 8, 30, UiKit.Hex(0xF2B93B));
                foreach (float x in new[] { 90f, 590f })
                {
                    UiKit.Shape(root, "Window", x, 250, 120, 100, UiKit.Hex(0xBFE3F2));
                    UiKit.Shape(root, "WindowBar", x + 56, 250, 8, 100, UiKit.Hex(0x9E9789));
                }
                return;
            }

            // 0: striped awning
            UiKit.Shape(root, "Wall", 40, 170, 720, 250, UiKit.Hex(0xEBCFA6));
            UiKit.Shape(root, "WallBase", 40, 170, 720, 26, UiKit.Hex(0xC9A777));

            UiKit.Shape(root, "DoorArch", 340, 256, 120, 112, WoodDark, round: true);
            UiKit.Shape(root, "Door", 340, 196, 120, 116, WoodDark);
            UiKit.Shape(root, "DoorGlass", 358, 210, 84, 130, UiKit.Hex(0xE9C27A));

            foreach (float x in new[] { 90f, 590f })
            {
                UiKit.Shape(root, "WindowFrame", x - 8, 228, 136, 116, WoodDark);
                UiKit.Shape(root, "Window", x, 236, 120, 100, UiKit.Hex(0xBFE3F2));
                UiKit.Shape(root, "WindowBar", x + 56, 236, 8, 100, WoodDark);
                UiKit.Shape(root, "SillCandy", x + 6, 238, 44, 33, Color.white, sprite: CandyArt.Candy(4));
                UiKit.Shape(root, "SillCandy", x + 70, 238, 44, 33, Color.white, sprite: CandyArt.Candy(6));
            }

            for (int i = 0; i < 12; i++)
                UiKit.Shape(root, "Awning", 40 + i * 60, 374, 60, 42, i % 2 == 0 ? UiKit.Hex(0xC7352F) : UiKit.Hex(0xFFF8EC));
            UiKit.Shape(root, "AwningEdge", 40, 366, 720, 10, UiKit.Hex(0x8E2420));
        }

        // The counter with a tray of lokum: 0 wood, 1 marble, 2 copper.
        private static void Counter(RectTransform root, int style)
        {
            Color legs, cloth, stripe, top, edge;
            switch (style)
            {
                case 1:
                    legs = UiKit.Hex(0xC9C2B7); cloth = UiKit.Hex(0xEFEBE4); stripe = UiKit.Hex(0x3E7CB1);
                    top = UiKit.Hex(0xF7F4EE); edge = UiKit.Hex(0xBDB6AB);
                    break;
                case 2:
                    legs = UiKit.Hex(0x8E4B1F); cloth = UiKit.Hex(0xB8672C); stripe = UiKit.Hex(0xE8A06A);
                    top = UiKit.Hex(0xC9783B); edge = UiKit.Hex(0x8E4B1F);
                    break;
                default:
                    legs = WoodDark; cloth = UiKit.Hex(0xC7352F); stripe = UiKit.Hex(0xFFF8EC);
                    top = Wood; edge = WoodDark;
                    break;
            }

            UiKit.Shape(root, "LegLeft", 190, 70, 24, 96, legs);
            UiKit.Shape(root, "LegRight", 426, 70, 24, 96, legs);
            UiKit.Shape(root, "Front", 176, 96, 288, 66, cloth);
            UiKit.Shape(root, "FrontStripe", 176, 126, 288, 12, stripe);
            if (style == 2)
            {
                for (int i = 0; i < 6; i++)
                    UiKit.Shape(root, "Rivet", 196 + i * 50, 104, 10, 10, UiKit.Hex(0xF4C095), round: true);
            }
            UiKit.Shape(root, "TableTop", 160, 150, 320, 34, top);
            UiKit.Shape(root, "TableEdge", 160, 150, 320, 10, edge);

            UiKit.Shape(root, "Tray", 200, 182, 240, 16, WoodDark);
            for (int i = 0; i < 4; i++)
                UiKit.Shape(root, "Candy", 204 + i * 58, 194, 54, 40, Color.white, sprite: CandyArt.Candy(i * 2 % CandyArt.TypeCount));
        }

        // Candy display on the counter: 0 glass case, 1 wooden shelves, 2 golden case.
        private static void Display(RectTransform root, int style)
        {
            if (style == 1)
            {
                UiKit.Shape(root, "PostLeft", 196, 184, 12, 124, WoodDark);
                UiKit.Shape(root, "PostRight", 432, 184, 12, 124, WoodDark);
                UiKit.Shape(root, "ShelfTop", 190, 296, 260, 12, WoodDark);
                UiKit.Shape(root, "ShelfMid", 196, 236, 248, 12, Wood);
                for (int i = 0; i < 4; i++)
                {
                    UiKit.Shape(root, "ShelfCandy", 212 + i * 56, 248, 50, 37, Color.white, sprite: CandyArt.Candy((i * 2 + 1) % CandyArt.TypeCount));
                    UiKit.Shape(root, "ShelfCandyTop", 212 + i * 56, 308, 50, 37, Color.white, sprite: CandyArt.Candy((i * 3 + 2) % CandyArt.TypeCount));
                }
                return;
            }

            bool golden = style == 2;
            UiKit.Shape(root, "CaseTray", 206, 236, 228, 14, WoodDark);
            for (int i = 0; i < 4; i++)
                UiKit.Shape(root, "CaseCandy", 210 + i * 56, 248, 50, 37, Color.white, sprite: CandyArt.Candy((i * 2 + 1) % CandyArt.TypeCount));

            var frame = golden ? UiKit.Hex(0xF2B93B) : Shine;
            UiKit.Shape(root, "GlassBody", 196, 184, 248, 118, Glass);
            UiKit.Shape(root, "GlassTop", 190, 296, 260, golden ? 16 : 12, frame);
            UiKit.Shape(root, "GlassLeft", 190, 184, golden ? 14 : 10, 124, frame);
            UiKit.Shape(root, "GlassRight", golden ? 436 : 440, 184, golden ? 14 : 10, 124, frame);
            if (golden)
                UiKit.Shape(root, "GlassCrest", 300, 304, 40, 18, UiKit.Hex(0xF2B93B), round: true);
            else
                UiKit.Shape(root, "GlassShine", 214, 262, 10, 34, UiKit.Hex(0xFFFFFF, 0.8f), rotation: 15f);
        }

        // The sign (its text is written by the game, not drawn): 0 wooden, 1 tile, 2 golden with a crown.
        private static void Sign(RectTransform root, int style, bool raised)
        {
            float y = raised ? 416f : 392f;
            const float boardHeight = 64f;

            Color board, rim, letters;
            switch (style)
            {
                case 1:
                    board = UiKit.Hex(0x1F5E9E); rim = UiKit.Hex(0x2F6DB5); letters = UiKit.Hex(0xFFF8EC);
                    break;
                case 2:
                    board = UiKit.Hex(0xC9921C); rim = UiKit.Hex(0xE9B83F); letters = UiKit.Hex(0xFFF2B0);
                    break;
                default:
                    board = WoodDark; rim = Wood; letters = UiKit.Hex(0xF6D36B);
                    break;
            }

            UiKit.Shape(root, "RopeLeft", 210, y - 30, 6, 40, UiKit.Hex(0x6B5535));
            UiKit.Shape(root, "RopeRight", 584, y - 30, 6, 40, UiKit.Hex(0x6B5535));
            UiKit.Shape(root, "BoardShadow", 188, y - 6, 424, boardHeight + 8, UiKit.Hex(0x000000, 0.16f));
            UiKit.Shape(root, "Board", 190, y, 420, boardHeight, board);
            UiKit.Shape(root, "BoardRim", 198, y + 7, 404, boardHeight - 14, rim);

            if (style == 1)
            {
                // a tile border: small light squares along the top and bottom edge
                for (int i = 0; i < 14; i++)
                {
                    UiKit.Shape(root, "TileTop", 200 + i * 29, y + boardHeight - 11, 14, 8, UiKit.Hex(0xFFF8EC));
                    UiKit.Shape(root, "TileBottom", 200 + i * 29, y + 3, 14, 8, UiKit.Hex(0xFFF8EC));
                }
            }
            if (style == 2)
            {
                UiKit.Shape(root, "CrownBase", 350, y + boardHeight + 1, 100, 11, UiKit.Hex(0xF2B93B));
                foreach (float x in new[] { 354f, 389f, 424f })
                    UiKit.Shape(root, "CrownPoint", x, y + boardHeight + 4, 22, 22, UiKit.Hex(0xF2B93B), rotation: 45f);
            }

            var text = UiKit.NewText("SignText", root, 40, FontStyle.Bold, TextAnchor.MiddleCenter, letters);
            UiKit.Place(text.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(400, y + boardHeight * 0.5f), new Vector2(404, 50));
            text.text = "Sweet Bazaar";
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // 0 table with two stools, 1 table under an umbrella, 2 cushions on a rug.
        private static void TeaCorner(RectTransform root, int style)
        {
            if (style == 2)
            {
                UiKit.Shape(root, "Rug", 530, 60, 220, 46, UiKit.Hex(0xA8322C));
                UiKit.Shape(root, "RugBorder", 538, 66, 204, 34, UiKit.Hex(0x3E7CB1));
                UiKit.Shape(root, "RugInner", 548, 72, 184, 22, UiKit.Hex(0xE9C27A));
                UiKit.Shape(root, "CushionLeft", 545, 96, 62, 34, UiKit.Hex(0x9A5BB5), round: true);
                UiKit.Shape(root, "CushionRight", 680, 96, 62, 34, UiKit.Hex(0x6FB04A), round: true);
                UiKit.Shape(root, "LowTray", 600, 100, 90, 14, UiKit.Hex(0xC9921C));
                TeaGlasses(root, 616, 114, 20);
                return;
            }

            if (style == 1)
            {
                // umbrella
                UiKit.Shape(root, "UmbrellaPole", 606, 120, 8, 190, WoodDark);
                UiKit.Shape(root, "UmbrellaTop", 520, 262, 180, 80, UiKit.Hex(0xC7352F), round: true);
                UiKit.Shape(root, "UmbrellaCover", 520, 250, 180, 40, UiKit.Hex(0xC7352F));
                for (int i = 0; i < 4; i++)
                    UiKit.Shape(root, "UmbrellaStripe", 536 + i * 40, 250, 14, 70, UiKit.Hex(0xFFF8EC, 0.9f));
            }

            UiKit.Shape(root, "TeaLeg", 600, 66, 16, 92, WoodDark);
            UiKit.Shape(root, "TeaFoot", 574, 60, 68, 12, WoodDark);
            UiKit.Shape(root, "TeaTop", 556, 150, 118, 30, Wood, round: true);
            UiKit.Shape(root, "StoolLeftLeg", 534, 40, 12, 40, WoodDark);
            UiKit.Shape(root, "StoolLeft", 512, 76, 58, 26, UiKit.Hex(0xC7352F), round: true);
            UiKit.Shape(root, "StoolRightLeg", 678, 40, 12, 40, WoodDark);
            UiKit.Shape(root, "StoolRight", 656, 76, 58, 26, UiKit.Hex(0x3E7CB1), round: true);
            TeaGlasses(root, 580, 168, 44);
        }

        private static void TeaGlasses(RectTransform root, float x, float y, float gap)
        {
            for (int i = 0; i < 2; i++)
            {
                float gx = x + i * gap;
                UiKit.Shape(root, "TeaGlass", gx, y, 24, 38, UiKit.Hex(0xC65D2C, 0.9f));
                UiKit.Shape(root, "TeaGlassRim", gx - 2, y + 32, 28, 8, UiKit.Hex(0xFFFFFF, 0.8f));
                UiKit.Shape(root, "Steam", gx + 4, y + 46, 14, 14, UiKit.Hex(0xFFFFFF, 0.5f), round: true);
                UiKit.Shape(root, "Steam", gx + 10, y + 64, 10, 10, UiKit.Hex(0xFFFFFF, 0.4f), round: true);
            }
        }

        // Decoration: 0 colourful bunting, 1 lanterns, 2 flower pots.
        private static void Decor(RectTransform root, int style, bool hasFacade)
        {
            float bunting = hasFacade ? 356f : 440f;

            if (style == 1)
            {
                var colors = new[] { 0xC7352F, 0xF2B93B, 0x3E7CB1, 0xE08A3C, 0x9A5BB5 };
                for (int i = 0; i < 7; i++)
                {
                    float x = 50 + i * 110;
                    float drop = 40 + (i % 3) * 26;
                    var color = UiKit.Hex(colors[i % colors.Length]);
                    UiKit.Shape(root, "LanternCord", x + 13, bunting - drop + 20, 4, drop, UiKit.Hex(0x6B5535));
                    UiKit.Shape(root, "LanternGlow", x - 14, bunting - drop - 36, 58, 82, UiKit.Hex(0xFFE9A8, 0.3f), round: true);
                    UiKit.Shape(root, "Lantern", x, bunting - drop - 22, 30, 52, color, round: true);
                    UiKit.Shape(root, "LanternCap", x + 5, bunting - drop + 22, 20, 8, UiKit.Hex(0xF2B93B));
                }
                return;
            }

            if (style == 2)
            {
                foreach (float x in new[] { 40f, 722f, 14f })
                {
                    float y = x < 100 ? 44f : 52f;
                    UiKit.Shape(root, "Leaf", x - 6, y + 38, 26, 36, UiKit.Hex(0x4C9A3C), round: true);
                    UiKit.Shape(root, "Leaf", x + 20, y + 40, 28, 40, UiKit.Hex(0x5FB04A), round: true);
                    UiKit.Shape(root, "Flower", x + 6, y + 64, 22, 22, UiKit.Hex(0xE76F8F), round: true);
                    UiKit.Shape(root, "Flower", x + 28, y + 56, 20, 20, UiKit.Hex(0xF2B93B), round: true);
                    UiKit.Shape(root, "Pot", x, y, 52, 44, UiKit.Hex(0xC9683A));
                    UiKit.Shape(root, "PotRim", x - 4, y + 36, 60, 12, UiKit.Hex(0xA8522C));
                }
                return;
            }

            var flags = new[] { 0xC7352F, 0xF2B93B, 0x3E7CB1, 0x6FB04A, 0x9A5BB5 };
            for (int i = 0; i < 14; i++)
                UiKit.Shape(root, "Flag", 30 + i * 52, bunting - 20 - (i % 2) * 6, 26, 26, UiKit.Hex(flags[i % flags.Length]), rotation: 45f);
            UiKit.Shape(root, "BuntingLine", 20, bunting, 760, 4, UiKit.Hex(0x6B5535));
        }
    }
}
