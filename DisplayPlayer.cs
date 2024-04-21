using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public abstract class DisplayPlayer
    {
        public Grid grid { get; protected set; }
        public abstract void displayGivenCards(SortedSet<Card> cards);

        protected DisplayPlayer(Grid grid)
        {
            this.grid = grid;
        }

        public void displayTrick()
        {

        }

        public void displayText(String name, int geraten, int stiche, int points)
        {
            TextBlock text = grid.Children.OfType<TextBlock>().First();
            if (geraten >= 0)
            {
                text.Text = name + "\n Geraten: " + geraten + ";  Erhalten: " + stiche + "; Punkte: " + points;
            }
            else
            {
                text.Text = name + "\n Noch nicht geraten;  Erhalten: " + stiche + "; Punkte: " + points;
            }

        }
    }
}
