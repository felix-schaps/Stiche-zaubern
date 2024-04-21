using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Stiche_zaubern
{
    public class Card : IComparable<Card>
    {
        public CardColor color { get; private set; }
        public int wert { get; private set; }
        private int id;
        public ImageSource getPictureSource()
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/" + CardColorExtensions.getColorName(this.color) + wert + ".jpg"));
        }

        public Card(CardColor color, int wert, int id = 0)
        {
            WrongCardException.check(color, wert);
            this.color = color;
            this.wert = wert;
            this.id = id;
        }

        public bool isJerk()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.JERK));
        }
        public bool isWizard()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.WIZARD));
        }

        public bool isBomb()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.BOMB));
        }

        public bool isDragon()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.DRAGON));
        }
        public bool isFairy()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.FAIRY));
        }

        public bool isJuggler()
        {
            return (color == CardColor.SPECIAL && wert == ((int)SpecialCard.JUGGLER));
        }

        public bool isFool()
        {
            return isJerk() || isFairy();
        }

        public bool isMagic()
        {
            return isWizard() || isDragon();
        }

        int IComparable<Card>.CompareTo(Card other)
        {
            int rank = this.color.CompareTo(other.color);
            if (rank != 0)
                return rank;
            rank = this.wert.CompareTo(other.wert);
            if (rank != 0)
                return rank;
            if (this.isJuggler() || other.isJuggler())
            {
                return (!this.isJuggler()).CompareTo(!other.isJuggler());
            }
            return this.id.CompareTo(other.id);
        }
    }

    public class WrongCardException : Exception
    {
        private WrongCardException() : base("Illegal card call or initialization.") { }
        public static void check(CardColor color, int wert)
        {
            if (wert <= 1 || wert > 13)
            {
                throw new WrongCardException();
            }
            if (color == CardColor.SPECIAL && wert > 7)
            {
                throw new WrongCardException();
            }
        }
    }
}
