using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class AIUnknownCardsHelper
    {
        public SortedSet<Card> UnknownCards { get; private set; }

        private SortedSet<Card> hand;

        public AIUnknownCardsHelper(SortedSet<Card> hand)
        {
            this.hand = hand;
            updateUnknownCards();
        }

        public double getProbHasColor(Player other, CardColor color)
        {
            if (color == CardColor.SPECIAL)
                return 1.0;

            List<Player> cannotFollow = Game.getActiveRound().getListOfKnownCannotFollow(color);

            if (cannotFollow.Contains(other))
                return 0.0;

            int cardsInDeck = UnknownCards.Count;
            int cardsOfOtherColor = cardsInDeck - getNumUnknownCardsOfColor(color);
            int cardsOnHand = hand.Count;

            return 1.0 - getProbAllCardsOfOtherColor(cardsOfOtherColor, cardsInDeck, cardsOnHand);
        }
        public double getProbHasWizard()
        {
            int cardsInDeck = UnknownCards.Count;
            int cardsOfOtherType = cardsInDeck - UnknownCards.Count(k => k.isWizard() || k.isDragon());
            int cardsOnHand = hand.Count;

            return 1.0 - getProbAllCardsOfOtherColor(cardsOfOtherType, cardsInDeck, cardsOnHand);
        }
        public double getProbHasTrump(Player other)
        {
            CardColor trump = Game.getActiveRound().TrumpColor;
            if (trump == CardColor.SPECIAL)
                return 0.0;
            return getProbHasColor(other, trump);
        }
        public int getNumUnknownCardsOfColor(CardColor color)
        {
            return UnknownCards.Count(k => k.color == color);
        }
        public int getNumExpectedDangerousCards()
        {
            if (UnknownCards.Count == 0)
                return 0;
            int danger = UnknownCards.Count(k => k.isWizard() || k.isDragon() || k.isBomb() || k.isJuggler());
            return danger * Game.getActiveRound().getNumCardsToLay() / UnknownCards.Count;
        }

        private double getProbAllCardsOfOtherColor(int cardsOfOtherColor, int cardsInDeck, int cardsOnHand)
        {
            double prob = 1.0;
            for (int j = 0; j < cardsOnHand; j++)
            {
                prob *= cardsOfOtherColor / (double)cardsInDeck;
                cardsInDeck--;
                cardsOfOtherColor--;
            }
            return prob;
        }
        private void updateUnknownCards()
        {
            SortedSet<Card> cards = new SortedSet<Card>(new Deck().stapel);
            GameRound activeRound = Game.getActiveRound();

            cards.Remove(activeRound.TrumpCard);
            hand.ToList().ForEach(k => cards.Remove(k));
            activeRound.getPlayedCards().ForEach(k => cards.Remove(k));

            UnknownCards = cards;
        }
    }

}