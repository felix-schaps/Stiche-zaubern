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
        public byte NrRound { get; set; }
        [Key(1)]
        public CardColor TrumpColor { get; set; }
        [Key(2)]
        public byte TrumpCardId { get; set; }
        [Key(3)]
        public RoundMode RoundMode { get; set; }
        [Key(4)]
        public List<PlayerInRound> PlayersInRound { get; set; }
        [Key(5)]
        public List<Trick> Tricks { get; set; }

    }

}
