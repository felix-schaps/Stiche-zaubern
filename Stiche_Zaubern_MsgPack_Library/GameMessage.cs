using System;
using System.Collections.Generic;
using System.Numerics;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{

    [MessagePackObject]
    public class DataMessage
    {
        [Key(0)]
        public byte MessageId;
        [Key(1)]
        public MessageType Type;
        [Key(2)]
        public byte[] Data;
    }

    [MessagePackObject]
    public class GameInfoMessage
    {
        [Key(0)]
        public List<byte> PlayerIds;
        [Key(1)]
        public List<string> PlayerNames;
        [Key(2)]
        public byte ActivePlayer;
    }

    [MessagePackObject]
    public class GamingMessage
    {
        [Key(0)]
        public RoundMode RoundMode;
        [Key(1)]
        public byte PlayerId;
        [Key(2)]
        public byte Arg;
    }

}
