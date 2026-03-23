using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources;

namespace Stiche_zaubern
{
    public sealed partial class NetworkConnectPage : Page
    {
        private CancellationTokenSource cts;
        private TcpClient client; // Zuordnung Clients
        private NetworkListener listener;
        private NetworkTalker talker ;

        public NetworkConnectPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            talker?.SendDisconnect();
            cts?.Cancel();
            client?.Close();
            listener = null;
            talker = null;
            client = null;
            Frame.Navigate(typeof(MainMenuPage));
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new TcpClient(txtbox_ip.Text, Convert.ToInt32(txtbox_port.Text));
                talker = new NetworkTalker(client.GetStream());
                listener = new NetworkListener(client.GetStream(), talker);
                await Task.Delay(20);
                talker.SendVersion();
                if (!listener.IsAccepted())
                {
                    Unaccept();
                    return;
                }

                var loader = ResourceLoader.GetForCurrentView();
                StatusText.Text = loader.GetString("ConnectionStatusConnected");

                StartButton.IsEnabled = false;
                talker.SendName(txtbox_playername.Text);
                List<Player> players = listener.GetPlayersOfGame();
                Frame.Navigate(typeof(GamePage));
                new GameManager(new GameClient(players, new NetworkTalkManager(new Dictionary<Player, NetworkTalker>() { { players[0], talker } }), listener));
            }
            catch(Exception)
            {   
                Frame.Navigate(typeof(NetworkConnectPage));
                StartButton.IsEnabled = true;
                Unaccept();
            }
        }

        private void Unaccept()
        {
            
            client = null;
            listener = null;
            talker = null;
            var loader = ResourceLoader.GetForCurrentView();
            StatusText.Text = loader.GetString("ConnectionStatusError");
        }
    }
}
