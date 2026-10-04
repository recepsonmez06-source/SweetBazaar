using UnityEngine;
using UnityEngine.UI;

namespace SweetBazaar.Game
{
    // The picture of the candy shop at a given stage, composed from simple shapes (placeholder art).
    // Stage 0 is a little counter; every higher stage keeps what was built before and adds something:
    // 1 display window, 2 shop sign, 3 tea corner, 4 big shop with a facade, 5 famous (golden sign, bunting, a queue).
    internal static class ShopPicture
    {
        public const float Width = 800f;
        public const float Height = 520f;

        private static readonly Color Wood = UiKit.Hex(0xA8723A);
        private static readonly Color WoodDark = UiKit.Hex(0x7A4B24);
        private static readonly Color Glass = UiKit.Hex(0xBFE3F2, 0.38f);

        // Fills `root` (Width x Height) with the picture. Existing children are removed first.
        public static void Build(RectTransform root, int stage)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                Objects.Dispose(root.GetChild(i).gameObject);

            Background(root);

            if (stage >= 4)
                Facade(root, stage);

            Counter(root);

            if (stage >= 1)
                DisplayWindow(root);
            if (stage >= 2)
                Sign(root, golden: stage >= 5, raised: stage >= 4);
            if (stage >= 3)
                TeaCorner(root);
            if (stage >= 5)
                Fame(root);
        }

        private static void Background(RectTransform root)
        {
            UiKit.Shape(root, "Sky", 0, 190, Width, 330, UiKit.Hex(0xCDE9F7), rotation: 0f);
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

        // Stage 4+: a big shop front behind the counter, with door, windows and a striped awning.
        private static void Facade(RectTransform root, int stage)
        {
            UiKit.Shape(root, "Wall", 40, 170, 720, 250, UiKit.Hex(0xEBCFA6));
            UiKit.Shape(root, "WallBase", 40, 170, 720, 26, UiKit.Hex(0xC9A777));

            UiKit.Shape(root, "DoorArch", 340, 256, 120, 112, UiKit.Hex(0x7A4B24), round: true);
            UiKit.Shape(root, "Door", 340, 196, 120, 116, UiKit.Hex(0x7A4B24));
            UiKit.Shape(root, "DoorGlass", 358, 210, 84, 130, UiKit.Hex(0xE9C27A));

            foreach (float x in new[] { 90f, 590f })
            {
                UiKit.Shape(root, "WindowFrame", x - 8, 228, 136, 116, WoodDark);
                UiKit.Shape(root, "Window", x, 236, 120, 100, UiKit.Hex(0xBFE3F2));
                UiKit.Shape(root, "WindowBar", x + 56, 236, 8, 100, WoodDark);
                // a few candies on the sill
                UiKit.Shape(root, "SillCandy", x + 6, 238, 44, 33, Color.white, sprite: CandyArt.Candy(4));
                UiKit.Shape(root, "SillCandy", x + 70, 238, 44, 33, Color.white, sprite: CandyArt.Candy(6));
            }

            // striped awning
            for (int i = 0; i < 12; i++)
                UiKit.Shape(root, "Awning", 40 + i * 60, 374, 60, 42, i % 2 == 0 ? UiKit.Hex(0xC7352F) : UiKit.Hex(0xFFF8EC));
            UiKit.Shape(root, "AwningEdge", 40, 366, 720, 10, UiKit.Hex(0x8E2420));
        }

        // The wooden counter with a tray of lokum on it (every stage).
        private static void Counter(RectTransform root)
        {
            UiKit.Shape(root, "LegLeft", 190, 70, 24, 96, WoodDark);
            UiKit.Shape(root, "LegRight", 426, 70, 24, 96, WoodDark);
            UiKit.Shape(root, "Cloth", 176, 96, 288, 66, UiKit.Hex(0xC7352F));
            UiKit.Shape(root, "ClothStripe", 176, 126, 288, 12, UiKit.Hex(0xFFF8EC));
            UiKit.Shape(root, "TableTop", 160, 150, 320, 34, Wood);
            UiKit.Shape(root, "TableEdge", 160, 150, 320, 10, WoodDark);

            UiKit.Shape(root, "Tray", 200, 182, 240, 16, WoodDark);
            for (int i = 0; i < 4; i++)
                UiKit.Shape(root, "Candy", 204 + i * 58, 194, 54, 40, Color.white, sprite: CandyArt.Candy(i * 2 % CandyArt.TypeCount));
        }

        // Stage 1: a glass case over the lokum.
        private static void DisplayWindow(RectTransform root)
        {
            UiKit.Shape(root, "CaseTray", 206, 236, 228, 14, WoodDark);
            for (int i = 0; i < 4; i++)
                UiKit.Shape(root, "CaseCandy", 210 + i * 56, 248, 50, 37, Color.white, sprite: CandyArt.Candy((i * 2 + 1) % CandyArt.TypeCount));

            UiKit.Shape(root, "GlassBody", 196, 184, 248, 118, Glass);
            UiKit.Shape(root, "GlassTop", 190, 296, 260, 12, UiKit.Hex(0xFFFFFF, 0.7f));
            UiKit.Shape(root, "GlassLeft", 190, 184, 10, 124, UiKit.Hex(0xFFFFFF, 0.7f));
            UiKit.Shape(root, "GlassRight", 440, 184, 10, 124, UiKit.Hex(0xFFFFFF, 0.7f));
            UiKit.Shape(root, "GlassShine", 214, 262, 10, 34, UiKit.Hex(0xFFFFFF, 0.8f), rotation: 15f);
        }

        // Stage 2: the sign with the shop's name; golden from stage 5; higher up when there is a big facade.
        private static void Sign(RectTransform root, bool golden, bool raised)
        {
            float y = raised ? 416f : 392f;
            const float boardHeight = 64f;
            var board = golden ? UiKit.Hex(0xC9921C) : WoodDark;
            var letters = golden ? UiKit.Hex(0xFFF2B0) : UiKit.Hex(0xF6D36B);

            UiKit.Shape(root, "RopeLeft", 210, y - 30, 6, 40, UiKit.Hex(0x6B5535));
            UiKit.Shape(root, "RopeRight", 584, y - 30, 6, 40, UiKit.Hex(0x6B5535));
            UiKit.Shape(root, "BoardShadow", 188, y - 6, 424, boardHeight + 8, UiKit.Hex(0x000000, 0.16f));
            UiKit.Shape(root, "Board", 190, y, 420, boardHeight, board);
            UiKit.Shape(root, "BoardRim", 198, y + 7, 404, boardHeight - 14, golden ? UiKit.Hex(0xE9B83F) : Wood);

            var text = UiKit.NewText("SignText", root, 40, FontStyle.Bold, TextAnchor.MiddleCenter, letters);
            UiKit.Place(text.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(400, y + boardHeight * 0.5f), new Vector2(404, 50));
            text.text = "Sweet Bazaar";
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // Stage 3: a little round table with two stools and tea.
        private static void TeaCorner(RectTransform root)
        {
            UiKit.Shape(root, "TeaLeg", 600, 66, 16, 92, WoodDark);
            UiKit.Shape(root, "TeaFoot", 574, 60, 68, 12, WoodDark);
            UiKit.Shape(root, "TeaTop", 556, 150, 118, 30, Wood, round: true);
            UiKit.Shape(root, "StoolLeftLeg", 534, 40, 12, 40, WoodDark);
            UiKit.Shape(root, "StoolLeft", 512, 76, 58, 26, UiKit.Hex(0xC7352F), round: true);
            UiKit.Shape(root, "StoolRightLeg", 678, 40, 12, 40, WoodDark);
            UiKit.Shape(root, "StoolRight", 656, 76, 58, 26, UiKit.Hex(0x3E7CB1), round: true);

            foreach (float x in new[] { 580f, 624f })
            {
                UiKit.Shape(root, "TeaGlass", x, 168, 24, 38, UiKit.Hex(0xC65D2C, 0.9f));
                UiKit.Shape(root, "TeaGlassRim", x - 2, 200, 28, 8, UiKit.Hex(0xFFFFFF, 0.8f));
                UiKit.Shape(root, "Steam", x + 4, 214, 14, 14, UiKit.Hex(0xFFFFFF, 0.5f), round: true);
                UiKit.Shape(root, "Steam", x + 10, 232, 10, 10, UiKit.Hex(0xFFFFFF, 0.4f), round: true);
            }
        }

        // Stage 5: a crown on the sign, bunting across the front, lanterns and a queue of customers.
        private static void Fame(RectTransform root)
        {
            // crown on the sign (the raised sign's top edge is at y = 480)
            UiKit.Shape(root, "CrownBase", 350, 481, 100, 11, UiKit.Hex(0xF2B93B));
            foreach (float x in new[] { 354f, 389f, 424f })
                UiKit.Shape(root, "CrownPoint", x, 484, 22, 22, UiKit.Hex(0xF2B93B), rotation: 45f);

            // bunting hanging just under the awning
            var colors = new[] { 0xC7352F, 0xF2B93B, 0x3E7CB1, 0x6FB04A, 0x9A5BB5 };
            for (int i = 0; i < 14; i++)
                UiKit.Shape(root, "Flag", 30 + i * 52, 336 - (i % 2) * 6, 26, 26, UiKit.Hex(colors[i % colors.Length]), rotation: 45f);
            UiKit.Shape(root, "BuntingLine", 20, 356, 760, 4, UiKit.Hex(0x6B5535));

            // lanterns beside the windows
            foreach (float x in new[] { 36f, 736f })
            {
                UiKit.Shape(root, "LanternCord", x + 12, 300, 4, 40, UiKit.Hex(0x6B5535));
                UiKit.Shape(root, "Lantern", x, 250, 28, 54, UiKit.Hex(0xF2B93B), round: true);
                UiKit.Shape(root, "LanternGlow", x - 12, 240, 52, 74, UiKit.Hex(0xFFE9A8, 0.35f), round: true);
            }

            // customers queueing at the counter
            var shirts = new[] { 0x3E7CB1, 0x6FB04A, 0x9A5BB5, 0xE08A3C };
            for (int i = 0; i < 4; i++)
            {
                float x = 40 + i * 36f;
                float height = 60f + (i % 2) * 8f;
                UiKit.Shape(root, "CustomerBody", x, 60, 34, height, UiKit.Hex(shirts[i]));
                UiKit.Shape(root, "CustomerHead", x + 1, 60 + height - 4, 32, 32, UiKit.Hex(0xF2C9A0), round: true);
            }
        }
    }
}
