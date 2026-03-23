using System;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class DeckException : Exception
    {
        private DeckException() : base("Deck too large") { }
        public static void Check(bool ex)
        {
            if (!ex)
            {
                throw new DeckException();
            }
        }
    }

    public class NotActiveGameRoundException : Exception
    {
        private NotActiveGameRoundException() : base("Another Game round is active at the moment.") { }
        public static void Check(GameRound round)
        {
            if (ActiveRoundInfo.getNumberRound() != round.NumberRound)
            {
                throw new NotActiveGameRoundException();
            }
        }
    }
    public class WrongGameModeException : Exception
    {
        private WrongGameModeException() : base("A different game mode as expected is active a the moment.") { }
        public static void Check(RoundMode expected, RoundMode actual)
        {
            if (expected != actual)
            {
                throw new WrongGameModeException();
            }
        }
    }
}