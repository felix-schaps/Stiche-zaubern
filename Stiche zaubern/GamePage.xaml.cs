using Stiche_Zaubern_MsgpLib;
using Windows.UI.Xaml;
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
            InitializeComponent();
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
            DisplayManager.GameCanvas = AnimationCanvas;

            button_raten_fertig.Visibility = Visibility.Collapsed;
            combo_raten.Visibility = Visibility.Collapsed;
        }

        private void button_raten_fertig_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveRoundInfo.getRoundMode() == RoundMode.END)
            {
                GameManager.cancelGame();
                return;
            }
            if (DisplayManager.getGameBoardComboBox().SelectedItem == null)
            {
                return;
            }
            GameManager.procInput((int)((ComboBoxItem)DisplayManager.getGameBoardComboBox().SelectedItem).Tag);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GameManager.cancelGame();
        }

    }
}
