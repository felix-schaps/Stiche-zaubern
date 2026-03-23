using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class NetworkTalkManager

    {
        private readonly Dictionary<Player, NetworkTalker> dic;
        private readonly bool isHost;

        public NetworkTalkManager(Dictionary<Player,NetworkTalker> dic, bool isHost = false)
        {
            this.dic = dic;
            this.isHost = isHost;
        }

        public void Talk(GamingMessageDecoder gameMessage)
        {
            if(!isHost && !(gameMessage.GetPlayer() is ActivePlayer))
            {
                return;
            }
            foreach(Player player in dic.Keys)
            {
                if(player != gameMessage.GetPlayer())
                {
                    dic[player].SendMessage(gameMessage.GamingMessage);
                }
            }
        }

        public void SendDeck(List<Card> mixedDeck)
        {
            if (!isHost)
            {
                return;
            }
            foreach (Player player in dic.Keys)
            {
                dic[player].SendDeck(mixedDeck);
            }
        }

        public void SendGame(Game game)
        {
            if (!isHost)
            {
                return;
            }
            foreach (Player player in dic.Keys)
            {
                dic[player].SendGame(game);
            }
        }

        public void SendDisconnect()
        {
            foreach (Player player in dic.Keys)
            {
                dic[player].SendDisconnect();
            }
        }
    }
}