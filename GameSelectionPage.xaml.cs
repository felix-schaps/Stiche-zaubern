using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class GameSelectionPage : Page
    {
        public GameSelectionPage()
        {
            this.InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamePage));

            List<Player> players = createPlayer();
            new Game(players);

            new GameManager();
        }

        private List<Player> createPlayer()
        {
            List<Player> list = new List<Player>();

            list.Add(new ActivePlayer(txtbox_playername.Text, 0, DisplayManager.GridActivePlayer));

            int ai_players = combo_ai_players.SelectedIndex + 2;
            for (int i = 0; i < ai_players; i++)
            {
                list.Add(new AIPlayer("AI-Player" + i, i + 1, DisplayManager.GridsOtherPlayers[i]));
            }

            return list;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
