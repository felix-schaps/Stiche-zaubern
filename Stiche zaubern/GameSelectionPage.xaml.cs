using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class GameSelectionPage : Page
    {
        public GameSelectionPage()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(GamePage));

            List<Player> players = createPlayer();
            _ = new GameManager(new GameServer(players, new NetworkTalkManager(new Dictionary<Player, NetworkTalker>())));
        }

        private List<Player> createPlayer()
        {
            List<Player> list = new List<Player>
            {
                new ActivePlayer(new Stiche_Zaubern_MsgpLib.Player() { Name = txtbox_playername.Text, Id = 0 }, DisplayManager.GridActivePlayer)
            };

            byte ai_players = (byte)(combo_ai_players.SelectedIndex + 2);
            for (byte i = 0; i < ai_players; i++)
            {
                list.Add(new AIPlayer(
                    new Stiche_Zaubern_MsgpLib.Player { Name = "AI-Player" + i, Id = (byte)(i + 1) },
                    DisplayManager.GridsOtherPlayers[i]
                ));
            }

            return list;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
