using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class AIPlayerInTricking : AIPlayerInTrick
    {
        public AIState State { get; private set; }
        public double Satisfaction { get; private set; }

        public AIPlayerInTricking(SortedSet<Card> hand, AIPlayerInRound aiPlayer) : base(hand, aiPlayer)
        {
        }

        public void updateAIState(int toGet, int overGuessing, bool beats, List<Player> otherPlayers)
        {
            State = calcAIState(toGet, overGuessing, beats, otherPlayers);
        }

        private AIState calcAIState(int toGet, int overGuessing, bool beats, List<Player> playersToCome)
        {
            if (toGet <= 0)
            {
                return AIState.NO_MORE_TRICK;
            }

            int unsafeTricks = aiCards.Count - NumWizards - NumFools;
            int expectedDanger = unknownHelper.getNumExpectedDangerousCards();
            int notBeating = beats ? 0 : 1;
            Satisfaction = NumWizards + unsafeTricks - toGet - expectedDanger - notBeating;
            if (Satisfaction <= 0.0)
            {
                return AIState.NEEDS_TRICK;
            }
            Satisfaction -= overGuessing;
            Satisfaction /= 2.0 * playersToCome.Count;
            Satisfaction += SumGuessing - toGet;
            if (toGet == 1 && expectedDanger == 0 && beats)
            {
                return AIState.CAUTIOUS;
            }

            if (toGet > SumGuessing)
            {
                return AIState.WANTS_TRICK;
            }

            return AIState.SATISFIED;
        }
    }
}