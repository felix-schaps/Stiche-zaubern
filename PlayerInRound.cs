using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class PlayerInRound
    {
        public SortedSet<Card> hand { get; private set; }
        public List<Trick> tricks { get; private set; }
        public int guessedTricks { get; internal set; }
        public Card chosenJugglingCard { get; internal set; }

        public PlayerInRound(Player player)
        {
            hand = new SortedSet<Card>();
            tricks = new List<Trick>();
            guessedTricks = -1;
            chosenJugglingCard = null;
        }

        public void giveCards(SortedSet<Card> cards)
        {
            hand = cards;
        }
        public int getNumberOfWonHands()
        {
            return tricks.Count;
        }
        public void giveTrick(Trick trick)
        {
            tricks.Add(trick);
        }
        public void popCard(Card popping)
        {
            if (!hand.Remove(popping))
                throw new Exception("Player has not the popping card.");
            if (hand.Contains(popping))
                throw new Exception("Popping card was not succesfull!");
        }
        public bool hasCardOfColor(CardColor color)
        {
            foreach (Card card in hand)
            {
                if (card.color == color)
                    return true;
            }
            return false;
        }
        public Dictionary<Card, bool> isLegalMove(Trick trick)
        {
            Dictionary<Card, bool> legalMoves = new Dictionary<Card, bool>();
            if (trick.followColor == CardColor.SPECIAL)
            {
                foreach (Card karte in hand)
                {
                    legalMoves.Add(karte, true);
                }
            }
            else
            {
                foreach (Card karte in hand)
                {
                    if (karte.color == CardColor.SPECIAL)
                    {
                        legalMoves.Add(karte, true);
                    }
                    else if (karte.color == trick.followColor)
                    {
                        legalMoves.Add(karte, true);
                    }
                    else
                    {
                        legalMoves.Add(karte, !hasCardOfColor(trick.followColor));
                    }
                }
            }
            return legalMoves;
        }
        public void giveJugglingCard(Card card)
        {
            hand.Add(card);
        }
        public int calculatePoints()
        {
            int diff = Math.Abs(guessedTricks - getNumberOfWonHands());
            if (diff == 0)
            {
                return 2 + getNumberOfWonHands();
            }
            else
            {
                return -diff;
            }
        }
        public bool hasGuessed()
        {
            return guessedTricks != -1;
        }
        public int getNumOfTricksToGet()
        {
            return Math.Max(0, guessedTricks - getNumberOfWonHands());
        }
    }
}
