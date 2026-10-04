using System;

namespace SweetBazaar.Core
{
    // The helps a player has in ONE level: how many moves can be undone and how many extra empty boxes can be added.
    // Starting over (restart) or a new level gives a fresh allowance. Later, a rewarded ad can add to it
    // (docs/TASARIM.md section 7); this class only keeps count.
    public sealed class Allowance
    {
        public const int DefaultUndos = 5;
        public const int DefaultExtraBoxes = 1;

        public Allowance(int undos = DefaultUndos, int extraBoxes = DefaultExtraBoxes)
        {
            if (undos < 0)
                throw new ArgumentOutOfRangeException(nameof(undos));
            if (extraBoxes < 0)
                throw new ArgumentOutOfRangeException(nameof(extraBoxes));

            UndosLeft = undos;
            ExtraBoxesLeft = extraBoxes;
        }

        public int UndosLeft { get; private set; }
        public int ExtraBoxesLeft { get; private set; }

        // True once any help has been used in this level (it is then no longer a "clean" solve).
        public bool AnyHelpUsed { get; private set; }

        public bool CanUndo => UndosLeft > 0;
        public bool CanAddExtraBox => ExtraBoxesLeft > 0;

        // Takes one undo; false (and nothing changes) if none are left.
        public bool TryUseUndo()
        {
            if (UndosLeft == 0)
                return false;

            UndosLeft--;
            AnyHelpUsed = true;
            return true;
        }

        public bool TryUseExtraBox()
        {
            if (ExtraBoxesLeft == 0)
                return false;

            ExtraBoxesLeft--;
            AnyHelpUsed = true;
            return true;
        }

        // Undoing an added extra box does not cost an undo; it gives the extra box back.
        public void GiveExtraBoxBack() => ExtraBoxesLeft++;

        // For rewarded ads and similar: adds helps.
        public void Grant(int undos, int extraBoxes)
        {
            if (undos < 0 || extraBoxes < 0)
                throw new ArgumentOutOfRangeException(undos < 0 ? nameof(undos) : nameof(extraBoxes));

            UndosLeft += undos;
            ExtraBoxesLeft += extraBoxes;
        }
    }
}
