using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class AITrumpChoosingCalculator
    {
        public static CardColor choose(ImmutableSortedSet<Card> cards)
        {
            int max = 0;
            CardColor argmax = CardColor.KREUZ;
            foreach (CardColor color in CardColorExtensions.getColors())
            {
                if (cards.Count(x => x.color == color) > max)
                {
                    max = cards.Count(x => x.color == color);
                    argmax = color;
                }
            }
            return argmax;
        }
    }
    public class AIJugglingCardCalculator
    {
        public static Card choose(AIPlayerInTricking helper, List<Card> karten)
        {
            Card juggling;
            CardsCalculations.seperateJerks(karten);
            Dictionary<Card, AICard> cards = helper.filterAndGetDictionary(karten);

            switch (helper.State)
            {
                case AIState.NEEDS_TRICK:
                case AIState.WANTS_TRICK:
                    juggling = AIBasicValueCalculator.getWeakestCard(cards);
                    break;
                case AIState.SATISFIED:
                    juggling = AIBasicValueCalculator.getKarteWithNearestAiValue(cards, helper.Satisfaction);
                    break;
                default:
                    juggling = AIBasicValueCalculator.getBestCard(cards);
                    break;
            }
            return juggling;
        }
    }

    public class AIBasicValueCalculator
    {
        private readonly Dictionary<Card, AICard> dicAICards;

        private AIBasicValueCalculator(Dictionary<Card, AICard> dicAICards)
        {
            this.dicAICards = dicAICards;
        }

        private Card getWeakestCard(bool bombing)
        {
            if (!bombing)
            {
                dropBomb();
            }

            double valMin = 100;
            Card argMin = null;
            foreach (Card karte in dicAICards.Keys)
            {
                if (dicAICards[karte].valueGuessing < valMin)
                {
                    valMin = dicAICards[karte].valueGuessing;
                    argMin = karte;
                }
            }
            return argMin ?? throw new Exception("Damn");
        }

        private void dropBomb()
        {
            if (dicAICards.Count > 1 && dicAICards.Keys.Any(k => k.isBomb()))
            {
                _ = dicAICards.Remove(dicAICards.Keys.First(k => k.isBomb()));
            }

            if (dicAICards.Count > 1 && dicAICards.Keys.Any(k => k.isJuggler()))
            {
                _ = dicAICards.Remove(dicAICards.Keys.First(k => k.isJuggler()));
            }
        }

        public static Card getWeakestCard(Dictionary<Card, AICard> cards, bool bombing = false)
        {
            return new AIBasicValueCalculator(cards).getWeakestCard(bombing);
        }
        public static Card getBestCard(Dictionary<Card, AICard> cards)
        {
            return new AIBasicValueCalculator(cards).getBestCard();
        }
        private Card getBestCard()
        {
            double valMax = -1.0;
            Card argMax = null;
            foreach (Card karte in dicAICards.Keys)
            {
                if (dicAICards[karte].valueGuessing > valMax)
                {
                    valMax = dicAICards[karte].valueGuessing;
                    argMax = karte;
                }
            }
            return argMax ?? throw new Exception("Damn");
        }

        private Card getKarteWithNearestAiValue(double val)
        {
            double nearest = 1000.0;
            Card argBest = null;
            foreach (Card karte in dicAICards.Keys)
            {
                AICard aiCard = dicAICards[karte];
                double winValue = aiCard.winningProb;
                if (Math.Abs(winValue - aiCard.valueGuessing - val) < nearest)
                {
                    nearest = Math.Abs(winValue - aiCard.valueGuessing - val);
                    argBest = karte;
                }
            }
            return argBest ?? throw new Exception("Damn");
        }
        public static Card getKarteWithNearestAiValue(Dictionary<Card, AICard> cards, double val)
        {
            return new AIBasicValueCalculator(cards).getKarteWithNearestAiValue(val);
        }
    }

}
