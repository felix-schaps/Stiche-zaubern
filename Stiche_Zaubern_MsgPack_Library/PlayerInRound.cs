using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class PlayerInRound
    {
        [Key(0)]
        public byte PlayerId;
        [Key(1)]
        public int NrRound;
        [Key(2)]
        public List<byte> Hand;
        [Key(3)]
        public bool HasChosenJugglingCard;
        [Key(4)]
        public byte ChosenJugglingCardId;
        [Key(5)]
        public int GuessedTricks;
        [Key(6)]
        public List<int> Tricks;
    }

}
