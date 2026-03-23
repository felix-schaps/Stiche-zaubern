using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class AICard
    {
        public double winningProb { get; private set; }
        public double valueGuessing { get; private set; }
        public readonly AIPlayerInRound player;

        private readonly Card card;


        public AICard(Card karte, AIPlayerInRound player)
        {
            card = karte;
            this.player = player;
        }

        public void setLosingIt()
        {
            winningProb = 0.0;
        }
        public void setWinningProbLive(List<PlayerInRound> playersToCome, AIUnknownCardsHelper helper)
        {
            winningProb = new AICardProbCalculatorWinning(card, new CardValueInUnknownCards(card, helper.UnknownCards), playersToCome, helper).result;
        }
        public void setGuessingValue(List<PlayerInRound> playersToCome, AIUnknownCardsHelper helper)
        {
            valueGuessing = new AICardProbCalculatorGuessing(card, new CardValueInUnknownCards(card, helper.UnknownCards), playersToCome).result;
        }
    }

}
