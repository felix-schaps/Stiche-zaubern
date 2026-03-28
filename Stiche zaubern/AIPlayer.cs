using System.Collections.Generic;
using System.Linq;
using Stiche_Zaubern_MsgpLib;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public enum AIState { NEEDS_TRICK, WANTS_TRICK, SATISFIED, NO_MORE_TRICK, CAUTIOUS }
    public class AIPlayer : Player
    {
        private AIPlayerInRound activeAI;

        public AIPlayer(Stiche_Zaubern_MsgpLib.Player playerLib, Grid position) : base(playerLib)
        {
            display = new DisplayOtherPlayer(position);
        }

        public Card chooseCard(Trick stich)
        {
            PlayerInRound playerInRound = getPlayerInActiveRound();
            List<PlayerInRound> players = getOtherPlayers();
            List<PlayerInRound> playersToCome = GameInfo.GetPlayers()
                .Where(p => !stich.hasLaidDown(p) && p != this)
                .Select(p => p.getPlayerInActiveRound()).ToList();

            List<Card> validCards = playerInRound.Hand.Where(x => playerInRound.isLegalMove(stich)[x]).ToList();
            List<Card> beatCards = validCards.Where(x => stich.beatenBy(x)).ToList();
            List<Card> losingCards = validCards.Where(x => !stich.beatenBy(x)).ToList();

            return activeAI.chooseCard(beatCards, losingCards, playersToCome, players);
        }
        public CardColor chooseTrump()
        {
            return AITrumpChoosingCalculator.choose(getPlayerInActiveRound().Hand);
        }
        public override void giveCards(List<Card> cards)
        {
            base.giveCards(cards);
            initializeNewRound();
        }
        public Card chooseJugglingCard()
        {
            return activeAI.chooseJugglingCard(getOtherPlayers());
        }
        public int guess(List<int> possibleGuesses)
        {
            List<PlayerInRound> otherPlayers = getOtherPlayers();
            double expectedTricks = activeAI.guess(otherPlayers);
            double variance = activeAI.getGuessingVariance();
            expectedTricks += variance / 2.0;

            return new NormalDistributionSelector().selectRandomValue(possibleGuesses, expectedTricks, variance);
        }

        private void initializeNewRound()
        {
            activeAI = new AIPlayerInRound(getPlayerInActiveRound());
        }

        private List<PlayerInRound> getOtherPlayers()
        {
            return ActiveRoundInfo.getOtherPlayers(getPlayerInActiveRound());
        }
    }

}
