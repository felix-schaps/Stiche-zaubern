using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public abstract class Game
    {
        public Game(List<Player> players, NetworkTalkManager talkManager)
        {
            Players = players;
            rounds = new List<GameRound>();
            new GameInfo(this);
            TalkManager = talkManager;
        }

        private readonly List<GameRound> rounds;

        public NetworkTalkManager TalkManager { get; private set; }
        public List<Player> Players;

        public GameRound GetActiveRound()
        {
            return rounds.Last();
        }
        public async Task<bool> BeginNewRound()
        {
            int nrNewRound = rounds.Count + 1;
            if (nrNewRound > 1 && GetActiveRound().IsEndGame())
            {
                return false;
            }

            rounds.Add(new GameRound(nrNewRound, Players));
            List<Card> mixedDeck = await TaskToGetMixedDeck();
            GetActiveRound().BeginRound(mixedDeck);

            return true;
        }

        protected virtual async Task<List<Card>> TaskToGetMixedDeck()
        {
            List<Card> mixedDeck = GameInfo.GetDeck().GetMixedDeck();
            TalkManager.SendDeck(mixedDeck);
            return mixedDeck;
        }

    }

    public class GameInfo
    {
        private readonly Game game;
        private readonly Deck deck;
        private static GameInfo Instance;

        public GameInfo(Game game)
        {
            this.game = game;
            deck = Deck.InitializieAndGetInstance();
            Instance = this;
        }

        public static Deck GetDeck()
            { return Instance.deck; }

        public static List<Player> GetPlayers()
        {
            return new List<Player>(Instance.game.Players);
        }
        public static int GetNumPlayers()
        {
            return Instance.game.Players.Count;
        }
        public static int GetMaxRounds()
        {
            return GetDeck().GetNumCards() / GetNumPlayers();
        }
        public static ActivePlayer GetActivePlayer()
        {
            foreach (Player player in GetPlayers())
            {
                if (player is ActivePlayer)
                {
                    return (ActivePlayer)player;
                }
            }
            throw new Exception("No active Player found.");
        }
        public static int GetPlacement(Player Player)
        {
            int pos = 1;
            int points = Player.points;

            foreach (Player sPlayer in GetPlayers())
            {
                if (sPlayer.points > points)
                {
                    pos++;
                }
            }
            return pos;
        }

        public static GameType GetGameType()
        {
            if (Instance.game is GameClient)
                return GameType.CLIENT_GAME;
            if (GetPlayers().Count(player => player is RemotePlayer) > 0)
                return GameType.HOST_GAME;
            return GameType.SINGLE_PLAYER;
        }
    }
}
