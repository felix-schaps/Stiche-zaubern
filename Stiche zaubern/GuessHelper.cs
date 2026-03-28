using System.Linq;

namespace Stiche_zaubern
{
    public class GuessHelper
    {
        private readonly GameRound _gameRound;

        public GuessHelper(GameRound gameRound)
        {
            _gameRound = gameRound;
        }

        public bool isGuessLegal(int guess)
        {
            int numberRound = _gameRound.NumberRound;
            int guessedSum = sumGuessed();

            if (numberRound == 1)
            {
                if (_gameRound.GetPlayersInRound().Count(p => p.hasGuessed()) + 2 == GameInfo.GetNumPlayers())
                {
                    return guess > 0 || guessedSum > 0;
                }
            }
            guessedSum += guess;
            return !isLastGuess() || (guessedSum != numberRound && guessedSum != numberRound - 1);
        }
        public bool isAllGuessed()
        {
            return _gameRound.GetPlayersInRound().Count(p => p.hasGuessed()) == GameInfo.GetNumPlayers();
        }
        public int getNumOverGuessing()
        {
            int toGet = 0;
            foreach (PlayerInRound player in _gameRound.GetPlayersInRound())
            {
                toGet += player.getNumOfTricksToGet();
            }
            return toGet - (_gameRound.NumberRound - _gameRound.ActiveTrick.Number + 1);
        }

        public double getOverGuessingNormalized()
        {
            int sum = 0;
            int guessed = 0;
            int numPlayers = 0;
            foreach (PlayerInRound player in _gameRound.GetPlayersInRound())
            {
                numPlayers++;
                if (player.hasGuessed())
                {
                    sum += player.GuessedTricks;
                    guessed++;
                }
            }
            return guessed == 0 ? 0.0 : (sum - (_gameRound.NumberRound * guessed / (double)numPlayers)) * guessed / (numPlayers - 1.0);
        }

        private bool isLastGuess()
        {
            return _gameRound.GetPlayersInRound().Count(p => p.hasGuessed()) + 1 == GameInfo.GetNumPlayers();
        }
        private int sumGuessed()
        {
            int sum = 0;
            foreach (PlayerInRound player in _gameRound.GetPlayersInRound())
            {
                if (player.hasGuessed())
                {
                    sum += player.GuessedTricks;
                }
            }
            return sum;
        }
    }
}
