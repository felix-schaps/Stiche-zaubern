using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Controls;
using Panel = Windows.UI.Xaml.Controls.Panel;

namespace Stiche_zaubern
{
    public class DisplayActivePlayer : DisplayPlayer
    {
        public DisplayActivePlayer(Grid position) : base(position) { }

        public override void displayGivenCards(SortedSet<Card> cards)
        {
            Panel panel = (Panel)((ScrollViewer)grid.Children[0]).Content;
            cards.ToList().ForEach(k => displayCard(k, panel));
        }

        private void displayCard(Card card, Panel panel)
        {
            Button button = new Button();
            Image image = new Image();
            image.Source = card.getPictureSource();
            image.Height = 100;
            button.Content = image;
            button.IsEnabled = false;
            button.Tag = card;
            button.Click += (sender, e) => reactTrick(button);
            panel.Children.Add(button);
        }

        private void reactTrick(Button button)
        {
            if (Game.getActiveRound().RoundMode == RoundMode.JUGGLING)
            {
                Player player = Game.getActivePlayer();
                Game.getActiveRound().procJuggle(player, (Card)button.Tag);
                Panel panel = (Panel)((ScrollViewer)grid.Children[0]).Content;
                panel.Children.Remove(button);
            }
            else
            {
                Player player = Game.getActivePlayer();
                Game.getActiveRound().ActiveTrick.addCardToTrick(player, (Card)button.Tag);
                Panel panel = (Panel)((ScrollViewer)grid.Children[0]).Content;
                panel.Children.Remove(button);
            }

            GameManager.procInput();
        }
    }
}
