using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class DisplayManager
    {
        public static Grid GridActivePlayer { get; set; }
        public static Grid[] GridsOtherPlayers { get; set; }
        public static Grid GridGameBoard { get; set; }
        public static Grid GridGameMenu { get; set; }

        public static ComboBox getGameBoardComboBox()
        {
            return GridGameBoard.Children.OfType<ComboBox>().First();
        }

        public static Button getGameBoardButton()
        {
            return GridGameBoard.Children.OfType<Button>().First();
        }

        public static Image getPicTrumpCard()
        {
            return GridGameBoard.Children.OfType<Image>().First(x => x.Name == "pic_trumpfkarte");
        }

        public static TextBlock getGameTextBlock()
        {
            return GridGameMenu.Children.OfType<TextBlock>().First();
        }

        public static void displayTexte()
        {
            foreach (Player player in Game.getPlayers())
            {
                string name = player.name;
                PlayerInRound playerInRound = player.getPlayerInActiveRound();
                int wonHands = playerInRound.getNumberOfWonHands();
                int points = player.points;
                int stiche = playerInRound.guessedTricks;
                player.display.displayText(name, stiche, wonHands, points);
            }
        }

        public static IEnumerable<Button> getActivePlayerButtons()
        {
            return ((StackPanel)((ScrollViewer)DisplayManager.GridActivePlayer.Children[0]).Content).Children.OfType<Button>();
        }
    }
}
