using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class AIPlayerInRound
    {
        private PlayerInRound player;

        private AIPlayerInTrick helper;
        private AIPlayerInTricking activeHelper;

        public AIPlayerInRound(PlayerInRound player)
        {
            this.player = player;
        }


        public Card chooseJugglingCard(List<Player> otherPlayers)
        {
            activeHelper = new AIPlayerInTricking(player.hand, this);
            activeHelper.updateAICards(otherPlayers);
            player.hand.ToList().ForEach(k => activeHelper.getAICard(k).setLosingIt());
            return AIJugglingCardCalculator.choose(activeHelper, player.hand.ToList());
        }

        public Card chooseCard(List<Card> beatCards, List<Card> losingCards, List<Player> playersToCome, List<Player> otherPlayers)
        {
            activeHelper = new AIPlayerInTricking(player.hand, this);
            activeHelper.updateAICards(otherPlayers);

            beatCards.ForEach(k => activeHelper.getAICard(k).setWinningProbLive(playersToCome, activeHelper.unknownHelper));
            losingCards.ForEach(k => activeHelper.getAICard(k).setLosingIt());

            bool beats = beatCards.Count > 0;
            int overGuessing = Game.getActiveGuessHelper().getNumOverGuessing();
            int toGet = player.getNumOfTricksToGet();
            activeHelper.updateAIState(toGet, overGuessing, beats, otherPlayers);

            Card candidateMagic = chooseMagicCard(toGet, beatCards);
            if (candidateMagic != null)
                return candidateMagic;

            if (activeHelper.State == AIState.CAUTIOUS)
                return chooseCardCautious(beatCards, losingCards, otherPlayers);

            return chooseCardByAIState(beatCards, losingCards, playersToCome);
        }

        private Card chooseMagicCard(int toGet, List<Card> beatCards)
        {
            if (toGet == activeHelper.NumWizards && beatCards.Count > activeHelper.NumWizards)
            {
                List<Card> list = beatCards.Where(k => k.isMagic() && activeHelper.getAICard(k).winningProb > 0.9).ToList();
                if (list.Count > 0)
                    return list.First();
            }
            return null;
        }

        private Card chooseCardCautious(List<Card> beatCards, List<Card> losingCards, List<Player> otherPlayers)
        {
            List<Card> usingCards = beatCards.Union(losingCards).ToList();
            List<Card> dangerousCardsToPlay = beatCards.Where(k => activeHelper.getAICard(k).winningProb > 0.5).ToList();

            if (usingCards.Count > dangerousCardsToPlay.Count)
            {
                AIPlayerInTrick simHelper = new AIPlayerInTrick(player.hand, this);
                dangerousCardsToPlay = dangerousCardsToPlay.Where(k => isCardStillDangerous(k, simHelper, otherPlayers)).ToList();
                usingCards.RemoveAll(k => dangerousCardsToPlay.Contains(k));
            }

            // has to play a dangerous card or there is no dangerous card
            double satisfaction = activeHelper.Satisfaction;
            return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), -satisfaction);
        }

        private bool isCardStillDangerous(Card card, AIPlayerInTrick simHelper, List<Player> otherPlayers)
        {
            List<Card> simHand = new List<Card>(player.hand);
            simHand.Remove(card);
            simHand.ForEach(k => simHelper.getAICard(k).setWinningProbLive(otherPlayers, simHelper.unknownHelper));
            return !(simHand.Any(k => simHelper.getAICard(k).winningProb < 0.5));
        }

        private Card chooseCardByAIState(List<Card> beatCards, List<Card> losingCards, List<Player> playersToCome)
        {
            bool beats = beatCards.Count > 0;
            List<Card> usingCards = beatCards.Union(losingCards).ToList();
            double unsatisfaction = - activeHelper.Satisfaction;
            switch (activeHelper.State)
            {
                case AIState.NEEDS_TRICK:
                    if (beats)
                    {
                        return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(beatCards), 100);
                    }
                    else
                    {
                        return AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(losingCards), true);
                    }
                case AIState.WANTS_TRICK:
                    if (beats)
                    {
                        return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), unsatisfaction);
                    }
                    else
                    {
                        return AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(losingCards), true);
                    }
                case AIState.SATISFIED:
                    return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), unsatisfaction);
                case AIState.NO_MORE_TRICK:
                    if (losingCards.Count > 0 && dropBomb(beatCards, losingCards))
                    {
                        return AIBasicValueCalculator.getBestCard(activeHelper.filterAndGetDictionary(losingCards));
                    }
                    else
                    {
                        return playersToCome.Count == 0 ? AIBasicValueCalculator.getBestCard(activeHelper.filterAndGetDictionary(beatCards)) : AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(beatCards));
                    }
                default:
                    throw new Exception("Damn");
            }
        }

        private bool dropBomb(List<Card> beatCards, List<Card> losingCards)
        {
            if (losingCards.TrueForAll(k => !k.isBomb()))
                return true;
            if (losingCards.Count > 1)
                return true;
            // If there is a beat card with low winning prob, then choose the beat card because bombing would imply to choose the next card in the next trick
            if (beatCards.Any(k => activeHelper.getAICard(k).winningProb < 0.5))
                return false;
            return true;
        }

        public double guess(List<Player> otherPlayers)
        {
            helper = new AIPlayerInTrick(player.hand, this);
            helper.updateAICards(otherPlayers);
            double overGuessing = Game.getActiveGuessHelper().getOverGuessingNormalized();
            return Math.Min(helper.SumGuessing - getGuessTwisting(overGuessing, otherPlayers.Count), player.hand.Count - helper.NumFools);
        }

        public double getGuessingVariance()
        {
            return 0.1;
        }

        private double getGuessTwisting(double overGuessing, int numOtherPlayers)
        {
            return overGuessing * 0.6 / Math.Sqrt(player.hand.Count);
        }
    }
}
