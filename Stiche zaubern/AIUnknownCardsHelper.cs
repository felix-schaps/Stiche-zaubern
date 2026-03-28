using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class AIUnknownCardsHelper
    {
        public SortedSet<Card> UnknownCards { get; private set; }

        private readonly PlayerInRound player;
        private readonly ImmutableSortedSet<Card> hand;

        public AIUnknownCardsHelper(PlayerInRound player)
        {
            this.player = player;
            hand = player.Hand;
            updateUnknownCards();
            this.player = player;
        }

        public double getProbHasColor(PlayerInRound other, CardColor color)
        {
            if (color == CardColor.SPECIAL)
            {
                return 1.0;
            }

            List<PlayerInRound> cannotFollow = ActiveRoundInfo.getListOfKnownCannotFollow(color);

            if (cannotFollow.Contains(other))
            {
                return 0.0;
            }

            int cardsInDeck = UnknownCards.Count;
            int cardsOfOtherColor = cardsInDeck - getNumUnknownCardsOfColor(color);
            int cardsOnHand = hand.Count;

            return 1.0 - getProbAllCardsOfOtherColor(cardsOfOtherColor, cardsInDeck, cardsOnHand);
        }

        public double getProbLaysWizard()
        {
            int numCardsInDeck = UnknownCards.Count;
            int numWizardCards = UnknownCards.Count(k => k.isWizard() || k.isDragon());
            int cardsOnHand = hand.Count;
            //Prob player has wizard
            double probHasWizard = 1.0 - getProbAllCardsOfOtherColor(numCardsInDeck - numWizardCards, numCardsInDeck, cardsOnHand);
            //real probability
            double probLayWizard = numWizardCards / (double)numCardsInDeck;
            return 0.5 * (probHasWizard + probLayWizard);
        }

        public double getProbHasTrump(PlayerInRound other)
        {
            CardColor trump = ActiveRoundInfo.getTrumpColor();
            return trump == CardColor.SPECIAL ? 0.0 : getProbHasColor(other, trump);
        }
        public int getNumUnknownCardsOfColor(CardColor color)
        {
            return UnknownCards.Count(k => k.color == color);
        }
        public int getNumExpectedDangerousCards()
        {
            if (UnknownCards.Count == 0)
            {
                return 0;
            }

            int danger = UnknownCards.Count(k => k.isWizard() || k.isDragon() || k.isBomb() || k.isJuggler());
            return danger * ActiveRoundInfo.getNumCardsToLay() / UnknownCards.Count;
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
            SortedSet<Card> cards = new SortedSet<Card>(GameInfo.GetDeck().GetMixedDeck());

            _ = cards.Remove(ActiveRoundInfo.getTrumpCard());
            hand.ToList().ForEach(k => cards.Remove(k));
            ActiveRoundInfo.getPlayedCards().ForEach(k => cards.Remove(k));

            foreach (CardColor color in CardColorExtensions.getColors())
            {
                if (ActiveRoundInfo.getOtherPlayers(player).ToHashSet().IsSubsetOf(ActiveRoundInfo.getListOfKnownCannotFollow(color).ToHashSet()))
                {
                    cards.Where(k => k.color == color).ToList().ForEach(k => cards.Remove(k));
                }
            }

            UnknownCards = cards;
        }
    }

}