using System;
using System.Collections.Generic;
using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class GameRound
    {
        public Trick ActiveTrick { get; private set; }
        public CardColor TrumpColor
        {
            get => _roundLib.TrumpColor;
            set
            {
                if (!IsChoosableTrumpf())
                {
                    throw new Exception("Illegal reset of Trumpf color");
                }
                _roundLib.TrumpColor = value;
                RoundMode = RoundMode.GUESSING;
            }
        }
        public Card TrumpCard
        {
            get => GameInfo.GetDeck().Decode(_roundLib.TrumpCardId);
            private set
            {
                _roundLib.TrumpCardId = GameInfo.GetDeck().Encode(value);
                _roundLib.TrumpColor = value.color;

                DisplayManager.getPicTrumpCard().Source = value.getPictureSource();
            }
        }
        public byte NumberRound { get => _roundLib.NrRound; private set => _roundLib.NrRound = value; }
        public RoundMode RoundMode { get => _roundLib.RoundMode; private set => _roundLib.RoundMode = value; }

        public GuessHelper GuessHelper { get; private set; }

        private Trick bombedTrick;
        private Dictionary<CardColor, List<PlayerInRound>> dicHelpColorCannotFollowSuitBy;
        private int beginWithPlayer;
        private Dictionary<Player, PlayerInRound> dicPlayersInRound;

        private Stiche_Zaubern_MsgpLib.GameRound _roundLib;

        public GameRound(byte numberRound, List<Player> players)
        {
            _roundLib = new Stiche_Zaubern_MsgpLib.GameRound();
            _ = new ActiveRoundInfo(this);
            NumberRound = numberRound;
            RoundMode = RoundMode.GUESSING;
            GuessHelper = new GuessHelper(this);
            bombedTrick = null;
            _roundLib.Tricks = new List<Stiche_Zaubern_MsgpLib.Trick>();
            initializePlayers(players);

            beginWithPlayer = (numberRound - 1) % GameInfo.GetNumPlayers();
        }

        public void BeginRound(List<Card> mixedDeck)
        {
            Queue<Card> activeStapel = new Queue<Card>(mixedDeck);
            giveCardsToPlayers(activeStapel);
            setTrumpCard(activeStapel);
        }

        public void ProcGuess(Player player, int guess)
        {
            NotActiveGameRoundException.Check(this);
            WrongGameModeException.Check(RoundMode, RoundMode.GUESSING);
            PlayerInRound playerInRound = dicPlayersInRound[player];
            if (playerInRound.hasGuessed())
            {
                throw new Exception("Already guessed.");
            }
            if (!GuessHelper.isGuessLegal(guess))
            {
                throw new Exception("Illegal guess");
            }
            playerInRound.GuessedTricks = guess;
        }
        public void ProcJuggle(Player player, Card card)
        {
            NotActiveGameRoundException.Check(this);
            WrongGameModeException.Check(RoundMode, RoundMode.JUGGLING);
            PlayerInRound playerInRound = dicPlayersInRound[player];
            if (playerInRound.chosenJugglingCard != null)
            {
                throw new Exception("Already juggled.");
            }
            playerInRound.chosenJugglingCard = card;
            player.popCard(card);
        }
        public bool IsChoosableTrumpf()
        {
            if (TrumpCard == null)
            {
                return false;
            }

            if (TrumpCard.isWizard() || TrumpCard.isDragon())
            {
                RoundMode = RoundMode.CHOOSING_TRUMP;
                return true;
            }
            RoundMode = RoundMode.GUESSING;
            return false;
        }
        public bool BeginTrick()
        {
            NotActiveGameRoundException.Check(this);

            if (ActiveTrick != null && ActiveTrick.Number == NumberRound)
            {
                computePoints();
                return false;
            }
            if (ActiveTrick != null && ActiveTrick.hasJuggler() && RoundMode != RoundMode.JUGGLING)
            {
                RoundMode = RoundMode.JUGGLING;
                return GetPlayersInRound().Any(p => p.chosenJugglingCard != null)
                    ? throw new Exception("Someone has already juggled a card.")
                    : false;
            }

            createNewTrick();
            return true;
        }
        public bool IsEndGame(bool set = true)
        {
            if (NumberRound + 1 > GameInfo.GetMaxRounds())
            {
                if (set)
                {
                    RoundMode = RoundMode.END;
                }

                return true;
            }
            return false;
        }
        public List<Card> GetPlayedCards()
        {
            List<Card> cards = new List<Card>();
            foreach(Stiche_Zaubern_MsgpLib.Trick trickLib in _roundLib.Tricks)
            {
                foreach (byte cardId in trickLib.Cards.Values)
                {
                    cards.Add(GameInfo.GetDeck().Decode(cardId));
                }
            }

            if (TrumpCard != null)
            {
                cards.Add(TrumpCard);
            }

            return cards;
        }
        public bool IsKnownCannotFollowSuit(CardColor color, PlayerInRound player)
        {
            List<PlayerInRound> players = dicHelpColorCannotFollowSuitBy[color];
            return players.Contains(player);
        }
        public List<PlayerInRound> GetListOfKnownCannotFollow(CardColor color)
        {
            return dicHelpColorCannotFollowSuitBy[color].ToList();
        }
        public void CannotFollowSuit(CardColor color, PlayerInRound player)
        {
            NotActiveGameRoundException.Check(this);
            List<PlayerInRound> players = dicHelpColorCannotFollowSuitBy[color];
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }
        public Queue<Player> GetActivePlayerQueue()
        {
            int numPlayers = GameInfo.GetNumPlayers();
            Queue<Player> queue = new Queue<Player>();

            for (int i = 0; i < numPlayers; i++)
            {
                int j = (i + beginWithPlayer) % numPlayers;
                queue.Enqueue(dicPlayersInRound.Keys.ToList()[j]);
            }

            return queue;
        }
        public void ProcTrick()
        {
            Player lucky = ActiveTrick.whoHasWonTrick();
            if (lucky == null)
            {
                bombedTrick = ActiveTrick;
                lucky = dicPlayersInRound.Keys.First(x => ActiveTrick.getCardOfPlayer(x).isBomb());
            }
            else
            {
               dicPlayersInRound[lucky].giveTrick(ActiveTrick);
            }
            beginWithPlayer = lucky.Id;
        }
        public PlayerInRound GetPlayerInRound(Player player)
        {
            return dicPlayersInRound[player];
        }
        public List<PlayerInRound> GetPlayersInRound()
        {
            return dicPlayersInRound.Values.ToList();
        }
        public int GetNumCardsToLay()
        {
            int tricksToPlay = NumberRound - ActiveTrick.Number;
            int numPlayers = dicPlayersInRound.Count;
            int cardsInActiveTrickToPlay = ActiveTrick.getNumPlayersToLay();
            return _ = (tricksToPlay * numPlayers) + cardsInActiveTrickToPlay;
        }

        private void initializePlayers(List<Player> players)
        {
            dicPlayersInRound = new Dictionary<Player, PlayerInRound>();
            dicHelpColorCannotFollowSuitBy = new Dictionary<CardColor, List<PlayerInRound>>();
            _roundLib.PlayersInRound = new List<Stiche_Zaubern_MsgpLib.PlayerInRound>();

            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                dicHelpColorCannotFollowSuitBy.Add(color, new List<PlayerInRound>());
            }

            foreach (Player player in players)
            {
                var playerInRoundLib = new Stiche_Zaubern_MsgpLib.PlayerInRound
                {
                    PlayerId = player.Id,
                    NrRound = NumberRound,
                    Hand = new List<byte>(),
                    HasChosenJugglingCard = false,
                    GuessedTricks = -1,
                    Tricks = new List<int>()
                };
                _roundLib.PlayersInRound.Add(playerInRoundLib);
                dicPlayersInRound.Add(player, new PlayerInRound(player, playerInRoundLib));
            }

            if (dicPlayersInRound.Count != GameInfo.GetNumPlayers())
            {
                throw new Exception("Initializing went wrong.");
            }
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
                {
                    trumpSet = true;
                }
            }

            if (!trumpSet)
            {
                TrumpCard = new Card(CardColor.SPECIAL, (int)SpecialCard.JERK, 5);
            }
        }
        private void computePoints()
        {
            NotActiveGameRoundException.Check(this);
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
            {
                numberNewTrick = ActiveTrick.Number + 1;
            }

            Stiche_Zaubern_MsgpLib.Trick trickLib = new Stiche_Zaubern_MsgpLib.Trick
            {
                Number = numberNewTrick,
                Cards = new Dictionary<byte, byte>()
            };
            _roundLib.Tricks.Add(trickLib);

            ActiveTrick = new Trick(this,trickLib);
        }

        public Stiche_Zaubern_MsgpLib.GameRound getGameRoundLib()
        {
            return _roundLib;
        }
    }

    
}
