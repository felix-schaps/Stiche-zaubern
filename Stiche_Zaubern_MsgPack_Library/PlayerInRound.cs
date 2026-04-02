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
        public byte PlayerId { get; set; }
        [Key(1)]
        public int NrRound { get; set; }
        [Key(2)]
        public List<byte> Hand { get; set; }
        [Key(3)]
        public bool HasChosenJugglingCard { get; set; }
        [Key(4)]
        public byte ChosenJugglingCardId { get; set; }
        [Key(5)]
        public int GuessedTricks { get; set; }
        [Key(6)]
        public List<int> Tricks { get; set; }
    }

}
