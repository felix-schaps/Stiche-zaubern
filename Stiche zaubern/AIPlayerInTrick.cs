using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class AIPlayerInTrick
    {
        public AIUnknownCardsHelper unknownHelper { get; private set; }
        public double SumGuessing { get; private set; }
        public int NumWizards;
        public int NumFools;

        protected Dictionary<Card, AICard> aiCards;
        protected CalculatorLogisticValue logCalc;

        public AIPlayerInTrick(PlayerInRound player, AIPlayerInRound aiPlayer)
        {
            aiCards = new Dictionary<Card, AICard>();
            player.hand.ToList().ForEach(k => aiCards.Add(k, new AICard(k, aiPlayer)));
            NumWizards = CardsCalculations.getNumWizardsOrDragons(player.hand.ToList());
            NumFools = CardsCalculations.getNumFools(player.hand.ToList());
            unknownHelper = new AIUnknownCardsHelper(player);
            logCalc = new CalculatorLogisticValue(1.5);
        }

        public void updateAICards(List<PlayerInRound> playersToCome)
        {
            SumGuessing = 0.0;

            foreach (Card card in aiCards.Keys)
            {
                aiCards[card].setGuessingValue(playersToCome, unknownHelper);
                SumGuessing += logCalc.calc(aiCards[card].valueGuessing);
            }

        }

        public AICard getAICard(Card card)
        {
            return aiCards[card];
        }

        public Dictionary<Card, AICard> filterAndGetDictionary(List<Card> cards)
        {
            return CardsCalculations.filterDictionary(aiCards, cards);
        }
    }
}