using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class Trick
    {
        [Key(0)]
        public int Number;
        [Key(1)]
        public Dictionary<byte, byte> Cards; //PlayerId, CardId

    }

}
