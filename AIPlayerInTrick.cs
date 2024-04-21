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

        public AIPlayerInTrick(SortedSet<Card> hand, AIPlayerInRound aiPlayer)
        {
            aiCards = new Dictionary<Card, AICard>();
            hand.ToList().ForEach(k => aiCards.Add(k, new AICard(k, aiPlayer)));
            NumWizards = CardsCalculations.getNumWizardsOrDragons(hand.ToList());
            NumFools = CardsCalculations.getNumFools(hand.ToList());
            unknownHelper = new AIUnknownCardsHelper(hand);
        }

        public void updateAICards(List<Player> playersToCome)
        {
            SumGuessing = 0.0;

            foreach (Card card in aiCards.Keys)
            {
                aiCards[card].setGuessingValue(playersToCome, unknownHelper);
                SumGuessing += aiCards[card].valueGuessing;
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