using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class Game
    {
        public static Game Instance { get; private set; }

        public Game(List<Player> players)
        {
            if (Instance != null)
            {
                Dispose();
            }
            Instance = this;
            this.players = players;
            deck = new Deck();
            rounds = new List<GameRound>();
        }

        private readonly Deck deck;
        private readonly List<GameRound> rounds;
        private readonly List<Player> players;

        public static ActivePlayer getActivePlayer()
        {
            foreach (Player player in Instance.players)
            {
                if (player is ActivePlayer)
                    return (ActivePlayer)player;
            }
            throw new Exception("No active Player found.");
        }
        public static List<Player> getPlayers()
        {
            return new List<Player>(Instance.players);
        }
        public static int getNumPlayers()
        {
            return Instance.players.Count;
        }
        public static int getMaxRounds()
        {
            return 60 / getNumPlayers();
        }
        public static GameRound getActiveRound()
        {
            return Instance.rounds.Last();
        }
        public static bool beginNewRound()
        {
            int nrNewRound = Instance.rounds.Count + 1;
            if (nrNewRound > 1 && getActiveRound().isEndGame())
            {
                return false;
            }

            Instance.rounds.Add(new GameRound(nrNewRound, Instance.players));
            getActiveRound().beginRound(Instance.deck.getMixedDeck());

            return true;
        }
        public static int getPlacement(Player Player)
        {
            int pos = 1;
            int points = Player.points;

            foreach (Player sPlayer in Instance.players)
            {
                if (sPlayer.points > points)
                    pos++;
            }
            return pos;
        }
        public static GuessHelper getActiveGuessHelper()
        {
            return getActiveRound().GuessHelper;
        }

        public void Dispose()
        {
            Instance = null;
        }
    }
}
