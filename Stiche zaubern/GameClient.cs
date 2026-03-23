

using System.IO;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stiche_zaubern
{
    public class GameClient : Game
    {
        private readonly NetworkListener _listener;

        public GameClient(List<Player> players, NetworkTalkManager talkManager, NetworkListener listener) : base(players, talkManager)
        {
            _listener = listener;
        }

        protected override async Task<List<Card>> TaskToGetMixedDeck()
        {
            return await _listener.TaskToGetMixedDeck();
        }
    }
}
