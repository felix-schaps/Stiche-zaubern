using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Panel = Windows.UI.Xaml.Controls.Panel;

namespace Stiche_zaubern
{
    public class DisplayActivePlayer : DisplayPlayer
    {
        public DisplayActivePlayer(Grid position) : base(position) { }

        public override void displayGivenCards(ImmutableSortedSet<Card> cards)
        {
            Panel panel = (Panel)((ScrollViewer)grid.Children[0]).Content;
            cards.ToList().ForEach(k => displayCard(k, panel));
        }

        private void displayCard(Card card, Panel panel)
        {
            Button button = new Button();
            Image image = new Image
            {
                Source = card.getPictureSource(),
                Height = 100
            };
            button.Content = image;
            button.IsEnabled = false;
            button.Tag = card;
            button.Click += (sender, e) => reactTrick(button);
            panel.Children.Add(button);
        }

        private void reactTrick(Button button)
        {
            object card = button.Tag;
            Panel panel = (Panel)((ScrollViewer)grid.Children[0]).Content;
            _ = panel.Children.Remove(button);
            GameManager.procInput(card);
        }
    }
}
