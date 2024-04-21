using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class GameRound
    {
        public Trick ActiveTrick { get; private set; }
        public CardColor TrumpColor
        {
            get
            {
                return _trumpColor;
            }
            set
            {
                if (!isChoosableTrumpf())
                {
                    throw new Exception("Illegal reset of Trumpf color");
                }
                _trumpColor = value;
                RoundMode = RoundMode.GUESSING;
            }
        }
        public Card TrumpCard
        {
            get
            {
                return _trumpCard;
            }
            private set
            {
                _trumpCard = value;
                _trumpColor = value.color;

                DisplayManager.getPicTrumpCard().Source = TrumpCard.getPictureSource();
            }
        }
        public int NumberRound { get; private set; }
        public RoundMode RoundMode { get; private set; }
        public GuessHelper GuessHelper { get; private set; }

        private Trick bombedTrick;
        private Dictionary<CardColor, List<Player>> dicHelpColorCannotFollowSuitBy;
        private int beginWithPlayer;
        private Dictionary<Player, PlayerInRound> dicPlayersInRound;
        private CardColor _trumpColor;
        private Card _trumpCard;

        public GameRound(int numberRound, List<Player> players)
        {
            this.NumberRound = numberRound;
            RoundMode = RoundMode.GUESSING;
            GuessHelper = new GuessHelper(this);
            bombedTrick = null;

            initializePlayers(players);

            beginWithPlayer = (numberRound - 1) % Game.getNumPlayers();
        }

        public void beginRound(List<Card> mixedDeck)
        {
            Queue<Card> activeStapel = new Queue<Card>(mixedDeck);
            giveCardsToPlayers(activeStapel);
            setTrumpCard(activeStapel);
        }

        public void procGuess(Player player, int guess)
        {
            NotActiveGameRoundException.check(this);
            WrongGameModeException.check(RoundMode, RoundMode.GUESSING);
            PlayerInRound playerInRound = dicPlayersInRound[player];
            if (playerInRound.hasGuessed())
            {
                throw new Exception("Already guessed.");
            }
            if (!GuessHelper.isGuessLegal(guess))
            {
                throw new Exception("Illegal guess");
            }
            playerInRound.guessedTricks = guess;
        }
        public void procJuggle(Player player, Card card)
        {
            NotActiveGameRoundException.check(this);
            WrongGameModeException.check(RoundMode, RoundMode.JUGGLING);
            PlayerInRound playerInRound = dicPlayersInRound[player];
            if (playerInRound.chosenJugglingCard != null)
            {
                throw new Exception("Already juggled.");
            }
            playerInRound.chosenJugglingCard = card;
            player.popCard(card);
        }
        public bool isChoosableTrumpf()
        {
            if (TrumpCard == null)
                return false;

            if (TrumpCard.isWizard() || TrumpCard.isDragon())
            {
                RoundMode = RoundMode.CHOOSING_TRUMP;
                return true;
            }
            RoundMode = RoundMode.GUESSING;
            return false;
        }
        public bool beginTrick()
        {
            NotActiveGameRoundException.check(this);

            if (dicPlayersInRound.First().Value.hand.Count == 0)
            {
                computePoints();
                return false;
            }
            if (ActiveTrick != null && ActiveTrick.hasJuggler() && RoundMode != RoundMode.JUGGLING)
            {
                RoundMode = RoundMode.JUGGLING;
                if (getPlayersInRound().Any(p => p.chosenJugglingCard != null))
                    throw new Exception("Someone has already juggled a card.");
                return false;
            }

            createNewTrick();
            return true;
        }
        public bool isEndGame(bool set = true)
        {
            if (NumberRound + 1 > Game.getMaxRounds())
            {
                if (set)
                    RoundMode = RoundMode.END;
                return true;
            }
            return false;
        }
        public List<Card> getPlayedCards()
        {
            List<Card> cards = new List<Card>();
            foreach (PlayerInRound player in getPlayersInRound())
            {
                foreach (Trick stich in player.tricks)
                {
                    cards = cards.Union(stich.getCards()).ToList();
                }
            }
            if (ActiveTrick != null)
            {
                foreach (Player player in dicPlayersInRound.Keys)
                {
                    if (ActiveTrick.hasLaidDown(player))
                    {
                        cards.Add(ActiveTrick.getCardOfPlayer(player));
                    }
                }
            }
            if (bombedTrick != null)
            {
                foreach (Player player in dicPlayersInRound.Keys)
                {
                    cards.Add(bombedTrick.getCardOfPlayer(player));
                }
            }

            if (TrumpCard != null)
                cards.Add(TrumpCard);

            return cards;
        }
        public bool isKnownCannotFollowSuit(CardColor color, Player player)
        {
            List<Player> players = dicHelpColorCannotFollowSuitBy[color];
            return players.Contains(player);
        }
        public List<Player> getListOfKnownCannotFollow(CardColor color)
        {
            return dicHelpColorCannotFollowSuitBy[color].ToList();
        }
        public void cannotFollowSuit(CardColor color, Player player)
        {
            List<Player> players = dicHelpColorCannotFollowSuitBy[color];
            if (!players.Contains(player))
                players.Add(player);
        }
        public Queue<Player> getActivePlayerQueue()
        {
            int numPlayers = Game.getNumPlayers();
            Queue<Player> queue = new Queue<Player>();

            for (int i = 0; i < numPlayers; i++)
            {
                int j = (i + beginWithPlayer) % numPlayers;
                queue.Enqueue(dicPlayersInRound.Keys.ToList()[j]);
            }

            return queue;
        }
        public void procTrick()
        {
            Player lucky = ActiveTrick.whoHasWonTrick();
            if (lucky == null)
            {
                bombedTrick = ActiveTrick;
                lucky = dicPlayersInRound.Keys.First(x => ActiveTrick.getCardOfPlayer(x).isBomb());
            }
            else
            {
                lucky.giveTrick(ActiveTrick);
            }
            beginWithPlayer = lucky.id;
        }
        public PlayerInRound getPlayerInRound(Player player)
        {
            return dicPlayersInRound[player];
        }
        public List<PlayerInRound> getPlayersInRound()
        {
            return dicPlayersInRound.Values.ToList();
        }
        public int getNumCardsToLay()
        {
            int tricksToPlay = NumberRound - ActiveTrick.Number;
            int numPlayers = dicPlayersInRound.Count;
            int cardsInActiveTrickToPlay = ActiveTrick.getNumPlayersToLay();
            return tricksToPlay = tricksToPlay * numPlayers + cardsInActiveTrickToPlay;
        }

        private void initializePlayers(List<Player> players)
        {
            dicPlayersInRound = new Dictionary<Player, PlayerInRound>();
            dicHelpColorCannotFollowSuitBy = new Dictionary<CardColor, List<Player>>();
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
                dicHelpColorCannotFollowSuitBy.Add(color, new List<Player>());
            foreach (Player player in players)
            {
                dicPlayersInRound.Add(player, new PlayerInRound(player));
            }

            if (dicPlayersInRound.Count != Game.getNumPlayers())
                throw new Exception("Initializing went wrong.");
        }
        private void giveCardsToPlayers(Queue<Card> deck)
        {
            foreach (Player player in dicPlayersInRound.Keys)
            {
                List<Card> cardsForPlayer = new List<Card>();
                for (int i = 0; i < NumberRound; i++)
                {
                    cardsForPlayer.Add(deck.Dequeue());
                }
                player.giveCards(cardsForPlayer);
            }
        }
        private void setTrumpCard(Queue<Card> deck)
        {
            bool trumpSet = false;
            while ((deck.Count > 0) && !trumpSet)
            {
                TrumpCard = deck.Dequeue();
                if (!TrumpCard.isJuggler() && !TrumpCard.isBomb())
                    trumpSet = true;
            }

            if (!trumpSet)
            {
                TrumpCard = new Card(CardColor.SPECIAL, (int)SpecialCard.JERK, 5);
            }
        }
        private void computePoints()
        {
            NotActiveGameRoundException.check(this);
            foreach (Player player in dicPlayersInRound.Keys)
            {
                player.updatePoints();
            }
        }
        private void createNewTrick()
        {
            RoundMode = RoundMode.TRICKING;

            int numberNewTrick = 1;
            if (ActiveTrick != null)
                numberNewTrick = ActiveTrick.Number + 1;

            ActiveTrick = new Trick(numberNewTrick);
        }
    }

    public class NotActiveGameRoundException : Exception
    {
        private NotActiveGameRoundException() : base("Another Game round is active at the moment.") { }
        public static void check(GameRound round)
        {
            if (Game.getActiveRound() != round)
            {
                throw new NotActiveGameRoundException();
            }
        }
    }
    public class WrongGameModeException : Exception
    {
        private WrongGameModeException() : base("A different game mode as expected is active a the moment.") { }
        public static void check(RoundMode expected, RoundMode actual)
        {
            if (expected != actual)
            {
                throw new WrongGameModeException();
            }
        }
    }
}
