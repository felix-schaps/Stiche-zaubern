using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public enum AIState { NEEDS_TRICK, WANTS_TRICK, SATISFIED, NO_MORE_TRICK, CAUTIOUS }
    public class AIPlayer : Player
    {
        private AIPlayerInRound activeAI;

        public AIPlayer(string name, int id, Grid position) : base(name, id)
        {
            base.display = new DisplayOtherPlayer(position);
        }

        public Card chooseCard(Trick stich)
        {
            PlayerInRound playerInRound = getPlayerInActiveRound();
            List<Player> players = getOtherPlayers();
            List<Player> playersToCome = players.Where(p => !stich.hasLaidDown(p)).ToList();

            List<Card> validCards = playerInRound.hand.Where(x => playerInRound.isLegalMove(stich)[x]).ToList();
            List<Card> beatCards = validCards.Where(x => stich.beatenBy(x)).ToList();
            List<Card> losingCards = validCards.Where(x => !stich.beatenBy(x)).ToList();

            return activeAI.chooseCard(beatCards, losingCards, playersToCome, players);
        }
        public CardColor chooseTrump()
        {
            return AITrumpChoosingCalculator.choose(getPlayerInActiveRound().hand);
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
            List<Player> otherPlayers = getOtherPlayers();
            double expectedTricks = activeAI.guess(otherPlayers);
            double variance = activeAI.getGuessingVariance();
            expectedTricks += variance / 2.0;

            return new NormalDistributionSelector().selectRandomValue(possibleGuesses, expectedTricks, variance);
        }

        private void initializeNewRound()
        {
            activeAI = new AIPlayerInRound(getPlayerInActiveRound());
        }

        private List<Player> getOtherPlayers()
        {
            List<Player> players = Game.getPlayers();
            players.Remove(this);
            return players;
        }
    }

}
