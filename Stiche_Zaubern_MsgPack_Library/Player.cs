using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public byte Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        public int Points { get; set; }
    }

}
