using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class SaveGame
    {
        [Key(0)]
        public List<Player> Players { get; set; }
        [Key(1)]
        public GameRound ActiveRound { get; set; }
        [Key(2)]
        public byte ActivePlayerId { get; set; }
        [Key(3)]
        public RoundMode RoundMode { get; set; }
        [Key(4)]
        public List<byte> PlayerQueue { get; set; }
        [Key(5)]
        public GameType GameType { get; set; }

    }

}
