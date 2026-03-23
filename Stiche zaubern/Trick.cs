using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class Trick
    {
        public CardColor followColor { get; private set; }
        public int Number { get; private set; }

        private readonly Dictionary<Player, Card> cards;
        private readonly GameRound round;

        public Trick(int number, GameRound round)
        {
            cards = new Dictionary<Player, Card>();
            followColor = CardColor.SPECIAL;
            Number = number;
            this.round = round;
        }

        public void addCardToTrick(Player key, Card karte)
        {
            NotActiveGameRoundException.Check(round);

            if (cards.ContainsKey(key))
            {
                throw new Exception("Player has already lain down cards.");
            }
            if (followColor == CardColor.SPECIAL)
            {
                followColor = karte.color;
            }

            cards.Add(key, karte);

            if (karte.color != followColor && karte.color != CardColor.SPECIAL)
            {
                round.CannotFollowSuit(followColor, key.getPlayerInActiveRound());
            }

            key.popCard(karte);
        }
        public bool hasJuggler()
        {
            return cards.Values.Any(x => x.isJuggler());
        }

        public bool hasDragon()
        {
            return cards.Values.Any(x => x.isDragon());
        }

        public bool hasWizard()
        {
            return cards.Values.Any(x => x.isWizard());
        }

        public bool hasFairy()
        {
            return cards.Values.Any(x => x.isFairy());
        }


        public Card getCardOfPlayer(Player player)
        {
            return !cards.ContainsKey(player) ? throw new Exception("Player has not lain down a card.") : cards[player];
        }
        public Player whoHasWonTrick()
        {
            if (cards.Count != GameInfo.GetNumPlayers())
            {
                throw new Exception("Not all players have lain down cards.");
            }

            Card wonCard = CardsCalculations.bestCard(cards.Values.ToList(), followColor);
            return wonCard == null ? null : cards.First(x => x.Value == wonCard).Key;
        }
        public bool beatenBy(Card karte)
        {
            if (cards.Count == 0)
            {
                return true;
            }

            List<Card> kartenliste = cards.Values.ToList();
            kartenliste.Add(karte);
            CardColor tempBedienfarbe = followColor;
            if (followColor == CardColor.SPECIAL)
            {
                tempBedienfarbe = karte.color;
            }
            Card won = CardsCalculations.bestCard(kartenliste, tempBedienfarbe);
            return won == karte;
        }
        public List<Card> getCards()
        {
            List<Card> cards = new List<Card>();
            foreach (Player player in GameInfo.GetPlayers())
            {
                Card card = getCardOfPlayer(player);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }
        public int getNumPlayersToLay()
        {
            return GameInfo.GetNumPlayers() - cards.Count;
        }
        public bool hasLaidDown(Player player)
        {
            return cards.ContainsKey(player);
        }
    }

}
