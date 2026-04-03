using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Stiche_Zaubern_MsgpLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class GameManager
    {
        public static int UPDATE_RATE { get; } = 50;
        public static int WAIT_THINKING { get; } = 500;
        public static int WAIT_TRICK_SHOW { get; } = 3000;

        private readonly static string filePath = "\\SaveGame.dat";
        private readonly static string localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

        private static GameManager Instance;
        private GameModeManager _gameModeManager;
        private readonly Game game;
        private readonly RequestHandler _requestHandler;
        private readonly NetworkTalkManager talkManager;

        /* WARNING ASYNC */
        public static void createGameModeManager<T>() where T : GameModeManager, new()
        {
            Instance._gameModeManager = new T();
            Instance._gameModeManager.begin(Instance.game.GetActiveRound(), Instance._requestHandler, Instance.talkManager);
        }

        public static void procInput(object arg)
        {
            Player activePlayer = GameInfo.GetActivePlayer();
            Instance._gameModeManager.resume(activePlayer, arg);
        }

        public static async void cancelGame()
        {
            DisplayManager.GridGameBoard.Visibility = Visibility.Collapsed;
            DisplayManager.getGameTextBlock().Text = "Spiel wird beendet.";

            Instance._requestHandler.CancelRequest = true;
            while(!Instance._requestHandler.IsCancelable)
            {
                await Task.Delay(UPDATE_RATE);
            }

            Instance.talkManager?.SendDisconnect();

            SaveGame();

            _ = Window.Current.Content is Frame frame ? frame.Navigate(typeof(MainMenuPage)) : throw new Exception("Cannot navigate");
            Instance.Dispose();
        }


        private static void SaveGame()
        {
            SaveGame saveGame = new SaveGame()
            {
                GameType = GameInfo.GetGameType(),
                Players = GameInfo.GetPlayers().Select(p => p.getPlayerLib()).ToList(),
                ActivePlayerId = GameInfo.GetActivePlayer().Id,
                ActiveRound = Instance.game.GetActiveRound().getGameRoundLib(),
                RoundMode = Instance.game.GetActiveRound().RoundMode,
                PlayerQueue = Instance._gameModeManager.GetPlayerQueue()
            };

        // Serialisieren und in Datei schreiben (korrekte MessagePack API verwenden)
        byte[] bytes = MessagePackSerializer.Serialize<SaveGame>(saveGame);
        System.IO.File.WriteAllBytes(localFolder + filePath, bytes);

            if (saveGame.RoundMode == RoundMode.END)
            {
                System.IO.File.Delete(localFolder + filePath);
            }
        }

        public static void skipAnimation()
        {
            if (Instance._requestHandler.IsSkipable)
            {
                Instance._requestHandler.SkipRequest = true;
            }
        }

        /* WARNING ASYNC */
        private static async Task procNewRound()
        {
            bool beginNewRound = await Instance.game.BeginNewRound();
            if (!beginNewRound)
            {
                createGameModeManager<GameEndingManager>();
                return;
            }

            if (Instance.game.GetActiveRound().IsChoosableTrumpf())
            {
                createGameModeManager<TrumpChoosingManager>();
                return;
            }

            createGameModeManager<GuessingManager>();
        }

        /* WARNING ASYNC */
        public static void procNewTrick()
        {
            if (!Instance.game.GetActiveRound().BeginTrick())
            {
                if (Instance.game.GetActiveRound().RoundMode == RoundMode.JUGGLING)
                {
                    createGameModeManager<JugglingManager>();
                    return;
                }
                else
                {
                   procNewRound();
                   return;
                }
            }
            createGameModeManager<TrickingManager>();
        }
        public GameManager(Game game)
        {
            if (Instance != null)
            {
                Dispose();
            }
            Instance = this;
            this.game = game;
            talkManager = game.TalkManager;
            _requestHandler = new RequestHandler();
            talkManager.SendGame(game);
            procNewRound();
        }

        public GameManager()
        {
            //Zum Laden eines Spiels aufgerufen
            if (Instance != null)
            {
                Dispose();
            }
            Instance = this;

            byte[] bindata = File.ReadAllBytes(localFolder + filePath);
            SaveGame data = MessagePackSerializer.Deserialize<SaveGame>(bindata);

            int numPlayers = data.Players.Count;

            List<Player> players = new List<Player>();
            foreach (Stiche_Zaubern_MsgpLib.Player playerLib in data.Players)
            {
                if (data.ActivePlayerId == playerLib.Id)
                {
                    players.Add(new ActivePlayer(playerLib, DisplayManager.GridActivePlayer));
                }
                else
                {
                    players.Add(new AIPlayer(playerLib, DisplayManager.GridsOtherPlayers[(playerLib.Id + data.ActivePlayerId - 1) % numPlayers]));
                }
            }

            this.game = new GameServer(players, new NetworkTalkManager(new Dictionary<Player, NetworkTalker>()));

            Dictionary<Player, List<Card>> playerHands = new Dictionary<Player, List<Card>>();
            foreach (Player player in game.Players)
            {
                Stiche_Zaubern_MsgpLib.PlayerInRound playerInRoundLib = data.ActiveRound.PlayersInRound.First(p => p.PlayerId == player.Id);
                playerHands.Add(player, playerInRoundLib.Hand.Select(b => GameInfo.GetDeck().Decode(b)).ToList());
            }

            game.SetActiveRound(data.ActiveRound);

            talkManager = game.TalkManager;
            _requestHandler = new RequestHandler();
            // Am Kontrollpunkt nach dem Laden des Spiels die GameModeManager entsprechend weiterfahren
            playerHands.ToList().ForEach(kv => kv.Key.giveCards(kv.Value));

            GameModeManager gameModeManager;
            switch (data.RoundMode)
            {
                case RoundMode.CHOOSING_TRUMP:
                    gameModeManager = new TrumpChoosingManager();
                    break;
                case RoundMode.GUESSING:
                    gameModeManager = new GuessingManager();
                    break;
                case RoundMode.TRICKING:
                    gameModeManager = new TrickingManager();
                    break;
                case RoundMode.JUGGLING:
                    gameModeManager = new JugglingManager();
                    break;
                default:
                    throw new Exception("Ungültiger Spielmodus");
            }
            Instance._gameModeManager = gameModeManager;

            Queue<Player> playerQueue = new Queue<Player>();

            foreach (var p in data.PlayerQueue)
            {
                playerQueue.Enqueue(players.First(pl => pl.Id == p));
            }

            Instance._gameModeManager.load(Instance.game.GetActiveRound(), Instance._requestHandler, Instance.talkManager, playerQueue);


        }

        public void Dispose()
        {
            Instance = null;
        }

        public static bool IsInitialized()
        {
            return Instance != null;
        }
    }

    public class GenericHelper<T>
    {
        public static ImmutableSortedSet<T> makeSet(params T[] values)
        {
            return values.ToImmutableSortedSet();
        }
    }
}