using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stiche_zaubern
{
    public class PlayerSlot
    {
        public TextBlock txt_block;
        public ComboBox combobox;
        public NetworkListener listener;
        public NetworkTalker talker;
        public TcpClient client;
        public SlotStatus status;
        public string name;
        public string text;


        public PlayerSlot(TextBlock textBlock, ComboBox comboBox)
        {
            txt_block = textBlock;
            combobox = comboBox;
            status = SlotStatus.CLOSED;
            text = "";
        }
    }

    public enum SlotStatus
    {
        OPEN_REMOTE, AI_PLAYER, REMOTE_PLAYER, CLOSED 
    }

    public sealed partial class NetworkHostPage : Page
    {
        private readonly int port = 5000;
        private CancellationTokenSource cts;
        private TcpListener server;
        private String statusTxt = "";
        private bool inGame;
        private PlayerSlot[] slots;
        private readonly ResourceLoader loader;

        public NetworkHostPage()
        {
            InitializeComponent();
            slots = new PlayerSlot[5];
            for (int i = 0; i < 5; i++)
                InitializePlayer(i);
            loader = ResourceLoader.GetForCurrentView();
            UpdateClientSlots();
        }

        private void InitializePlayer(int i)
        {
            switch(i)
            {
                case 0:
                    slots[0] = new PlayerSlot(txt_status2, combo_player2);
                    break;
                case 1:
                    slots[1] = new PlayerSlot(txt_status3, combo_player3);
                    break;
                case 2:
                    slots[2] = new PlayerSlot(txt_status4, combo_player4);
                    break;
                case 3:
                    slots[3] = new PlayerSlot(txt_status5, combo_player5);
                    break;
                case 4:
                    slots[4] = new PlayerSlot(txt_status6, combo_player6);
                    break;
                default:
                    throw new Exception("Wrong function call.");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TextIP.Text = "IP-Adresse: " + GetLocalIPAddress();
            TextPort.Text = "Port: " + port;
            inGame = false;
            int aport = 5000;
            server = new TcpListener(IPAddress.Any, aport);
            server.Start();
            statusTxt = string.Format(loader.GetString("ServerStarted"), port);
            new Thread(() => iniClients()).Start();
        }

        // Prüft die ComboBoxen und zählt, wie viele Clients benötigt werden
        private void UpdateClientSlots()
        {
            StatusText.Text = statusTxt;
            if (slots == null)
                return;
            for(int i = 0; i< slots.Length; i++)
            {
                PlayerSlot slot = slots[i];
                slot.txt_block.Text = slot.text;
                if(slot.txt_block==null)
                {
                    return;
                }
                
                string current = (slot.combobox.SelectedItem as ComboBoxItem)?.Name;
                if (current.Contains("Remote") && slot.status != SlotStatus.REMOTE_PLAYER)
                {
                    slot.txt_block.Text = loader.GetString("WaitingForClient");
                    slot.status = SlotStatus.OPEN_REMOTE;
                }
                else if (!current.Contains("Remote"))
                {
                    if (slot.status == SlotStatus.REMOTE_PLAYER)
                    {
                        slot.txt_block.Text = string.Format(loader.GetString("PlayerRemoved"), slot.name);
                        slot.talker.SendDisconnect();
                        InitializePlayer(i);
                    }
                    slot.txt_block.Text = "";
                    if (!current.Contains("Closed"))
                    {
                        slot.status = SlotStatus.AI_PLAYER;
                    }
                    else
                        slot.status = SlotStatus.CLOSED;
                }
            }

            if (!isFreeSeat())
            {
                StartButton.IsEnabled = true;
                cts?.Cancel();
            }
            else if (StartButton.IsEnabled)
            {
                StartButton.IsEnabled = false;
                new Thread(() => iniClients()).Start();
            }

        }

        private bool isFreeSeat()
        {
            return slots.Any(slot => slot.status == SlotStatus.OPEN_REMOTE);
        }

        private PlayerSlot getFreeSeat()
        {
            return slots.First(slot => slot.status == SlotStatus.OPEN_REMOTE);
        }


        // Event für ComboBox-Änderungen
        private void combo_player3_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateClientSlots();
        private void combo_player4_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateClientSlots();
        private void combo_player5_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateClientSlots();
        private void combo_player6_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateClientSlots();

        private async void iniClients()
        {


            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            

            try
            {
                while (isFreeSeat())
                {
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateClientSlots());

                    var acceptTask = server.AcceptTcpClientAsync();
                    var completedTask = await Task.WhenAny(acceptTask, Task.Delay(-1, token)); // Wartet entweder auf Client oder auf Abbruch

                    if (completedTask == acceptTask)
                    {
                        TcpClient client = acceptTask.Result;
                        PlayerSlot slot = getFreeSeat();
                        if (slot == null)
                            break;
                        slot.status = SlotStatus.REMOTE_PLAYER;
                        slot.client = client;
                        slot.talker = new NetworkTalker(client.GetStream());
                        slot.listener = new NetworkListener(client.GetStream(), slot.talker);
                        if (!slot.listener.IsAccepted())
                        {
                            slot.talker.SendAcceptance(false);
                            slot.talker = null;
                            slot.client = null;
                            slot.listener = null;
                            slot.status = SlotStatus.OPEN_REMOTE;
                            continue;
                        }
                        slot.talker.SendAcceptance();
                        slot.text = string.Format(loader.GetString("PlayerConnected"), slot.listener.GetName());
                    }
                    else
                    {
                        break;
                    }
                }
                statusTxt = loader.GetString("AllClientsConnected");
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateClientSlots());
            }
            catch (Exception)
            {
                statusTxt = loader.GetString("ConnectionError");
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateClientSlots());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (PlayerSlot slot in slots)
                slot.talker?.SendDisconnect();
            cts?.Cancel();
            server?.Stop();
            Frame.Navigate(typeof(MainMenuPage));
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            inGame = true;
            _ = Frame.Navigate(typeof(GamePage));

            createGame();
        }

        private void createGame()
        {
            ComboBox[] playerCombos = { combo_player2, combo_player3, combo_player4, combo_player5, combo_player6 };
            List<Player> players = new List<Player>
            {
                new ActivePlayer(txtbox_playername.Text, 0, DisplayManager.GridActivePlayer)
            };
            byte iRemotePlayer = 0;
            byte iAIPlayer = 0;
            byte iTotalPlayer = 1;
            var talkDic = new Dictionary<Player, NetworkTalker>();
            foreach (PlayerSlot slot in slots)
            {
                if (slot.status == SlotStatus.REMOTE_PLAYER)
                {
                    Player player = new RemotePlayer(slot.listener.GetName(), iTotalPlayer, DisplayManager.GridsOtherPlayers[iTotalPlayer - 1], slot.listener);
                    players.Add(player);
                    talkDic.Add(player, slot.talker);
                    iRemotePlayer++;
                }
                else if (slot.status == SlotStatus.AI_PLAYER)
                {
                    iAIPlayer++;
                    players.Add(new AIPlayer("AI-Player" + iAIPlayer, iTotalPlayer, DisplayManager.GridsOtherPlayers[iTotalPlayer - 1]));
                }
                iTotalPlayer++;
            }

            new GameManager(new GameServer(players, new NetworkTalkManager(talkDic, true)));
        }
    

  
        private string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("IP-Adresse nicht ermittelt.");
        }

    }
}
