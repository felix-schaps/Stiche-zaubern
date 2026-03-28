using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class PlayerInRound
    {
        public ImmutableSortedSet<Card> Hand { 
            get => _hand; 
            private set {
                _playerInRoundLib.Hand = value.Select(card => GameInfo.GetDeck().Encode(card)).ToList();
                _hand = value;
            }
        }

        public int GuessedTricks { get => _playerInRoundLib.GuessedTricks; internal set => _playerInRoundLib.GuessedTricks = value; }
        public Card chosenJugglingCard { 
            get => _playerInRoundLib.HasChosenJugglingCard ? GameInfo.GetDeck().Decode(_playerInRoundLib.ChosenJugglingCardId) : null;
            internal set
            {
                if (value != null)
                {
                    _playerInRoundLib.ChosenJugglingCardId = GameInfo.GetDeck().Encode(value);
                    _playerInRoundLib.HasChosenJugglingCard = true;
                }
                else
                {
                    _playerInRoundLib.HasChosenJugglingCard = false;
                }
            } }

        private Stiche_Zaubern_MsgpLib.PlayerInRound _playerInRoundLib;

        private List<Trick> tricks;
        private ImmutableSortedSet<Card> _hand;

        public PlayerInRound(Player player, Stiche_Zaubern_MsgpLib.PlayerInRound playerInRoundLib)
        {
            _playerInRoundLib = playerInRoundLib;
            tricks = new List<Trick>();
        }

        public void giveCards(ImmutableSortedSet<Card> cards)
        {
            Hand = cards;
        }
        public int getNumberOfWonHands()
        {
            return tricks.Count;
        }
        public void giveTrick(Trick trick)
        {
            tricks.Add(trick);
            _playerInRoundLib.Tricks.Add(trick.Number);
        }
        public void popCard(Card popping)
        {
            byte id = GameInfo.GetDeck().Encode(popping);
            if (!_playerInRoundLib.Hand.Remove(id))
            {
                throw new Exception("Player has not the popping card.");
            }

            updateHand();

            if (Hand.Contains(popping))
            {
                throw new Exception("Popping card was not succesfull!");
            }
        }
        public bool hasCardOfColor(CardColor color)
        {
            foreach (Card card in Hand)
            {
                if (card.color == color)
                {
                    return true;
                }
            }
            return false;
        }
        public Dictionary<Card, bool> isLegalMove(Trick trick)
        {
            Dictionary<Card, bool> legalMoves = new Dictionary<Card, bool>();

            if(trick.hasDragon()&&trick.hasWizard()&&Hand.Any(k => k.isFairy()))
            {
                foreach (Card karte in Hand)
                {
                    legalMoves.Add(karte, karte.isFairy());
                }
                return legalMoves;
            }


            if (trick.followColor == CardColor.SPECIAL)
            {
                foreach (Card karte in Hand)
                {
                    legalMoves.Add(karte, true);
                }
            }
            else
            {
                foreach (Card karte in Hand)
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
            byte id = GameInfo.GetDeck().Encode(card);
            _playerInRoundLib.Hand.Add(id);
            updateHand();
        }
        public int calculatePoints()
        {
            int fairyTale = tricks.Any(trick => trick.hasDragon() && trick.hasFairy()) ? 1 : 0;

            int diff = Math.Abs(GuessedTricks - getNumberOfWonHands());
            return diff == 0 ? 2 + getNumberOfWonHands()+fairyTale : -diff+fairyTale;
        }
        public bool hasGuessed()
        {
            return GuessedTricks != -1;
        }
        public int getNumOfTricksToGet()
        {
            return Math.Max(0, GuessedTricks - getNumberOfWonHands());
        }

        private void updateHand()
        {
            _hand = _playerInRoundLib.Hand.Select(id => GameInfo.GetDeck().Decode(id)).ToImmutableSortedSet();
        }
    }
}
