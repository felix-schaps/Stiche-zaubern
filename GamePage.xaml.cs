using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Stiche_zaubern
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DisplayManager.GridActivePlayer = grid_active_player;
            DisplayManager.GridsOtherPlayers = new Grid[5];
            DisplayManager.GridsOtherPlayers[0] = grid_player2;
            DisplayManager.GridsOtherPlayers[1] = grid_player3;
            DisplayManager.GridsOtherPlayers[2] = grid_player4;
            DisplayManager.GridsOtherPlayers[3] = grid_player5;
            DisplayManager.GridsOtherPlayers[4] = grid_player6;
            DisplayManager.GridGameBoard = grid_game_board;
            DisplayManager.GridGameMenu = grid_menu;
        }

        private void button_raten_fertig_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Game.getActiveRound().RoundMode == RoundMode.END)
            {
                endGame();
                return;
            }
            GameManager.procInput();
        }

        private void BackButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            endGame();
        }

        private async void endGame()
        {
            DisplayManager.GridGameBoard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DisplayManager.getGameTextBlock().Text = "Spiel wird beendet.";
            await GameManager.Instance.CurrentTask;
            Game.Instance.Dispose();
            GameManager.Instance.Dispose();
            Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
