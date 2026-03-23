using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class AIPlayerInRound
    {
        private readonly PlayerInRound player;

        private AIPlayerInTrick helper;
        private AIPlayerInTricking activeHelper;

        public AIPlayerInRound(PlayerInRound player)
        {
            this.player = player;
        }


        public Card chooseJugglingCard(List<PlayerInRound> otherPlayers)
        {
            activeHelper = new AIPlayerInTricking(player, this);
            activeHelper.updateAICards(otherPlayers);
            player.hand.ToList().ForEach(k => activeHelper.getAICard(k).setLosingIt());
            return AIJugglingCardCalculator.choose(activeHelper, player.hand.ToList());
        }

        public Card chooseCard(List<Card> beatCards, List<Card> losingCards, List<PlayerInRound> playersToCome, List<PlayerInRound> otherPlayers)
        {
            activeHelper = new AIPlayerInTricking(player, this);
            activeHelper.updateAICards(otherPlayers);

            beatCards.ForEach(k => activeHelper.getAICard(k).setWinningProbLive(playersToCome, activeHelper.unknownHelper));
            losingCards.ForEach(k => activeHelper.getAICard(k).setLosingIt());

            bool beats = beatCards.Count > 0;
            int overGuessing = ActiveRoundInfo.getActiveGuessHelper().getNumOverGuessing();
            int toGet = player.getNumOfTricksToGet();
            activeHelper.updateAIState(toGet, overGuessing, beats, otherPlayers);

            Card candidateMagic = chooseMagicCard(toGet, beatCards);
            return candidateMagic ?? (activeHelper.State == AIState.CAUTIOUS
                ? chooseCardCautious(beatCards, losingCards, otherPlayers)
                : chooseCardByAIState(beatCards, losingCards, playersToCome));
        }

        private Card chooseMagicCard(int toGet, List<Card> beatCards)
        {
            if (toGet == activeHelper.NumWizards && beatCards.Count > activeHelper.NumWizards)
            {
                List<Card> list = beatCards.Where(k => k.isMagic() && activeHelper.getAICard(k).winningProb > 0.9).ToList();
                if (list.Count > 0)
                {
                    return list.First();
                }
            }

            return beatCards.Find(c => c.isFairy());
        }

        private Card chooseCardCautious(List<Card> beatCards, List<Card> losingCards, List<PlayerInRound> otherPlayers)
        {
            List<Card> usingCards = beatCards.Union(losingCards).ToList();
            List<Card> dangerousCardsToPlay = beatCards.Where(k => activeHelper.getAICard(k).winningProb > 0.5 || k.isJuggler()).ToList();

            if (usingCards.Count > dangerousCardsToPlay.Count)
            {
                AIPlayerInTrick simHelper = new AIPlayerInTrick(player, this);
                dangerousCardsToPlay = dangerousCardsToPlay.Where(k => isCardStillDangerous(k, simHelper, otherPlayers)).ToList();
                _ = usingCards.RemoveAll(k => dangerousCardsToPlay.Contains(k));
            }

            // has to play a dangerous card or there is no dangerous card
            double satisfaction = activeHelper.Satisfaction;
            return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), -satisfaction);
        }

        private bool isCardStillDangerous(Card card, AIPlayerInTrick simHelper, List<PlayerInRound> otherPlayers)
        {
            List<Card> simHand = new List<Card>(player.hand);
            _ = simHand.Remove(card);
            simHand.ForEach(k => simHelper.getAICard(k).setWinningProbLive(otherPlayers, simHelper.unknownHelper));
            return !simHand.Any(k => simHelper.getAICard(k).winningProb < 0.5);
        }

        private Card chooseCardByAIState(List<Card> beatCards, List<Card> losingCards, List<PlayerInRound> playersToCome)
        {
            bool beats = beatCards.Count > 0;
            List<Card> usingCards = beatCards.Union(losingCards).ToList();
            double unsatisfaction = -activeHelper.Satisfaction;
            switch (activeHelper.State)
            {
                case AIState.NEEDS_TRICK:
                    return beats
                        ? AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(beatCards), 100)
                        : (losingCards.Any(k=>k.isJuggler()) ? 
                            losingCards.First(k =>k.isJuggler()) : AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(losingCards), true));
                case AIState.WANTS_TRICK:
                    return beats
                        ? AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), unsatisfaction)
                        : AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(losingCards), true);
                case AIState.SATISFIED:
                    if (usingCards.Count > 0 && usingCards.Any(k => k.isJuggler()))
                    {
                        _ = usingCards.RemoveAll(k => k.isJuggler());
                    }
                    return AIBasicValueCalculator.getKarteWithNearestAiValue(activeHelper.filterAndGetDictionary(usingCards), unsatisfaction);
                case AIState.NO_MORE_TRICK:
                    if (unsatisfaction > 1 && losingCards.Any(k => k.isJuggler()))
                        return losingCards.First(k => k.isJuggler());
                    return losingCards.Count > 0 && dropBomb(beatCards, losingCards)
                        ? AIBasicValueCalculator.getBestCard(activeHelper.filterAndGetDictionary(losingCards))
                        : playersToCome.Count == 0 ? AIBasicValueCalculator.getBestCard(activeHelper.filterAndGetDictionary(beatCards)) : AIBasicValueCalculator.getWeakestCard(activeHelper.filterAndGetDictionary(beatCards));
                default:
                    throw new Exception("Damn");
            }
        }

        private bool dropBomb(List<Card> beatCards, List<Card> losingCards)
        {
            if (losingCards.TrueForAll(k => !k.isBomb()))
            {
                return true;
            }

            if (losingCards.Count > 1)
            {
                return true;
            }
            // If there is a beat card with low winning prob, then choose the beat card because bombing would imply to choose the next card in the next trick
            return !beatCards.Any(k => activeHelper.getAICard(k).winningProb < 0.5);
        }

        public double guess(List<PlayerInRound> otherPlayers)
        {
            helper = new AIPlayerInTrick(player, this);
            helper.updateAICards(otherPlayers);
            double overGuessing = ActiveRoundInfo.getActiveGuessHelper().getOverGuessingNormalized();
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
