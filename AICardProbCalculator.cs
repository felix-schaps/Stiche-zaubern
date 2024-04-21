using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public abstract class AICardProbCalculator
    {
        public double result { get; protected set; }
    }

    public class AICardProbCalculatorGuessing : AICardProbCalculator
    {
        public AICardProbCalculatorGuessing(Card card, CardValueInUnknownCards valueInfo, List<Player> playersToCome)
        {
            int beaten = valueInfo.beaten.Count;
            int beats = valueInfo.beats.Count;
            int allMatters = beaten + beats;
            result = allMatters == 0 ? 0.0 : Math.Pow(beats / (double)allMatters, playersToCome.Count);
        }
    }
    public class AICardProbCalculatorWinning : AICardProbCalculator
    {
        public AICardProbCalculatorWinning(Card card, CardValueInUnknownCards valueInfo, List<Player> playersToCome, AIUnknownCardsHelper helper)
        {
            if (playersToCome.Count == 0)
            {
                result = 1.0;
            }

            if (card.color == CardColor.SPECIAL)
            {
                result = calculateSimple(card, valueInfo, playersToCome);
                return;
            }

            result = calculateComplex(card, valueInfo, playersToCome, helper);
        }

        private double calculateSimple(Card card, CardValueInUnknownCards valueInfo, List<Player> playersToCome)
        {
            int beaten = valueInfo.beaten.Count;
            int all = beaten + valueInfo.beats.Count + valueInfo.orderMatters.Count;
            if (all == 0)
                return 1.0;
            double probBeatenPerPerson = beaten / (double)all;
            return Math.Pow(1.0 - probBeatenPerPerson, playersToCome.Count);
        }

        private double calculateComplex(Card card, CardValueInUnknownCards valueInfo, List<Player> playersToCome, AIUnknownCardsHelper helper)
        {
            double prob = 1.0;
            foreach (Player player in playersToCome)
            {
                double probFollow = helper.getProbHasColor(player, card.color);
                int numColor = helper.getNumUnknownCardsOfColor(card.color);
                int numBeatenColor = valueInfo.beaten.Count(k => k.color == card.color);
                if (numColor > 0)
                    prob *= (1.0 - probFollow * numBeatenColor / numColor);
                
                if (card.color != Game.getActiveRound().TrumpColor)
                {
                    prob *= 1.0 - (1.0 - probFollow) * helper.getProbHasTrump(player);
                }
                prob *= 1.0 - helper.getProbHasWizard();
            }
            return prob;
        }
    }
}
