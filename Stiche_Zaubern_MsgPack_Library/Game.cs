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
        public List<Player> Players;
        [Key(1)]
        public GameRound ActiveRound;
        [Key(2)]
        public byte ActivePlayerId;
        [Key(3)]
        public RoundMode RoundMode;
        [Key(4)]
        public List<byte> PlayerQueue;
        [Key(5)]
        public GameType GameType;

    }

}
