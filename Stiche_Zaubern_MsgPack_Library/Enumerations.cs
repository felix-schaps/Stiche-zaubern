using System;
using System.Collections.Generic;
using System.Text;

namespace Stiche_Zaubern_MsgpLib
{
    public enum MessageType { CONNECTED, READY, STARTING_GAME, LEFT_GAME, DECK, GAMING, DELIVERED }

    public enum RoundMode { CHOOSING_TRUMP, GUESSING, TRICKING, JUGGLING, END }

    public enum GameType { SINGLE_PLAYER , HOST_GAME, CLIENT_GAME}

    public enum CardColor
    {
        KARO, HERZ, PIK, KREUZ, SPECIAL
    }
}
