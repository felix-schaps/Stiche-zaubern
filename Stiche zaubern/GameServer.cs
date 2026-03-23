using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Stiche_zaubern
{
    public class GameServer : Game
    {
        public GameServer(List<Player> players, NetworkTalkManager talkManager) : base(players, talkManager) { }
    }
}
