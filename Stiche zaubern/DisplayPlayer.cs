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

        public void displayText(string name, int geraten, int stiche, int points)
        {
            TextBlock text = grid.Children.OfType<TextBlock>().First();
            text.Text = geraten >= 0
                ? name + "\n"+ DisplayManager.GetString("txt_trick") + stiche + "/"+ geraten + "\n" + DisplayManager.GetString("txt_points") + points
                : name + "\n"+ DisplayManager.GetString("txt_not-guessed") +"\n" + DisplayManager.GetString("txt_points") + points;

        }
    }
}
