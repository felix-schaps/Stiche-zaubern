using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class GameRound
    {
        [Key(0)]
        public byte NrRound;
        [Key(1)]
        public CardColor TrumpColor;
        [Key(2)]
        public byte TrumpCardId;
        [Key(3)]
        public RoundMode RoundMode;
        [Key(4)]
        public List<PlayerInRound> PlayersInRound;
        [Key(5)]
        public List<Trick> Tricks;

    }

}
