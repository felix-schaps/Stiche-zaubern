using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class CardValueInUnknownCards
    {
        public List<Card> orderMatters { get; private set; }
        public List<Card> beaten { get; private set; }
        public List<Card> beats { get; private set; }

        public CardValueInUnknownCards(Card card, SortedSet<Card> unknownCards)
        {
            this.beaten = new List<Card>();
            this.orderMatters = new List<Card>();
            this.beats = new List<Card>();

            foreach (Card other in unknownCards)
            {
                if (isWinning(other, card, card))
                    beats.Add(other);
                else if (!isWinning(card, other, card))
                    beaten.Add(other);
                else
                    orderMatters.Add(other);
            }
        }

        private bool isWinning(Card first, Card second, Card thisCard)
        {
            List<Card> helperList = new List<Card> { first, second };
            CardColor bedienfarbe = (first.color != CardColor.SPECIAL) ? first.color : second.color;

            return (CardsCalculations.bestCard(helperList, bedienfarbe) == thisCard);
        }
    }

}