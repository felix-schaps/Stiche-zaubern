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
        public byte Id;
        [Key(1)]
        public string Name;
        [Key(2)]
        public int Points;
    }

}
