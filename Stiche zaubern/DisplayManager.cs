using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class DisplayManager
    {
        public static Grid GridActivePlayer { get; set; }
        public static Grid[] GridsOtherPlayers { get; set; }
        public static Grid GridGameBoard { get; set; }
        public static Grid GridGameMenu { get; set; }
        public static Canvas GameCanvas { get; set; }

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

        public static void displayTexts()
        {
            foreach (Player player in GameInfo.GetPlayers())
            {
                string name = player.Name;
                PlayerInRound playerInRound = player.getPlayerInActiveRound();
                int wonHands = playerInRound.getNumberOfWonHands();
                int points = player.Points;
                int stiche = playerInRound.GuessedTricks;
                player.display.displayText(name, stiche, wonHands, points);
            }
        }

        public static string GetString(string identifier)
        {
            return Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(identifier);
        }

        public static IEnumerable<Button> getActivePlayerButtons()
        {
            return ((StackPanel)((ScrollViewer)GridActivePlayer.Children[0]).Content).Children.OfType<Button>();
        }
    }
}
