using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MessagePack;
using Stiche_Zaubern_MsgpLib;
using Windows.ApplicationModel;
using Windows.Storage.Streams;

namespace Stiche_zaubern
{

    public class NetworkListener
    {
        public NetworkStream stream { get; private set; }
        private Queue<DataMessage> messages = new Queue<DataMessage>();
        private NetworkTalker talker;
        private List<Player> players;
        private bool? accepted;
        private int counter = 0;
        private bool stop = false;

        public NetworkListener(NetworkStream stream, NetworkTalker talker)
        {
            this.stream = stream;
            accepted = null;
            new Thread(() => listen()).Start();
            this.talker = talker;
        }

        public async Task<List<Card>> TaskToGetMixedDeck()
        {
            List<Card> list = new List<Card>();
            while(!messages.Any(m => m.Type == MessageType.DECK))
            {
                await Task.Delay(1000);
            }
            DataMessage dataMsg = messages.First(m => m.Type == MessageType.DECK);
            byte[] mixedDeck = dataMsg.Data;
            for (int i = 0; i < mixedDeck.Length; i++)
            {
                list.Add(GameInfo.GetDeck().Decode(mixedDeck[i]));
            }
            messages.TryDequeue(out dataMsg);
            return list;
        }

        public async Task<object> TaskToGetMessage(RoundMode mode,Player player)
        {
            if (stop) throw new SocketException(-2);
            while(messages.Count() == 0)
            {
                await Task.Delay(1000);
            }
            DataMessage dataMsg = messages.Dequeue();
            if (dataMsg.Type != MessageType.GAMING)
                throw new Exception("Unexpected");
            GamingMessageDecoder message = new GamingMessageDecoder(MessagePackSerializer.Deserialize<GamingMessage>(dataMsg.Data));
            if(message.GetRoundMode() != mode || message.GetPlayer() != player)
            {
                throw new IOException("Something went wrong.");
            }
            return message.GetArg();
        }

        private void listen()
        {
            try
            {
                while (true)
                {
                    int read = 0;
                    byte[] lengthBuffer = new byte[4];
                    read = stream.Read(lengthBuffer, 0, 4);
                    if (read <= 0) throw new IOException("Not read enough.");
                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] messageBuffer = new byte[messageLength];
                    int bytesRead = 0;
                    while (bytesRead < messageLength)
                    {
                        read = stream.Read(messageBuffer, bytesRead, messageLength - bytesRead);
                        if (read <= 0) throw new IOException("Not read enough.");
                        bytesRead += read;
                    }
                    if (messageLength > 2)
                    {
                        DataMessage dataMsg = MessagePackSerializer.Deserialize<DataMessage>(messageBuffer);
                        talker.SendDelivery(dataMsg.MessageId);
                        messages.Enqueue(dataMsg);
                        return;
                    }
                    else if (messageBuffer[0] == (byte)MessageType.DELIVERED)
                    {
                        talker.ReceiveDelivered(messageBuffer[1]);
                    }
                    else if (messageBuffer[0] == (byte)MessageType.CONNECTED)
                    {
                        if (messageBuffer[1] == 1)
                        {
                            accepted = true;
                        }
                        else if (messageBuffer[1] == 2)
                        {
                            accepted = false;
                        }
                    }
                    else
                    {
                        throw new SocketException();
                    }
                }
            }
            catch(SocketException)
            {
                stop = true;
            }
            catch(IOException)
            {
                stop = true;
            }
        }


        public string GetName()
        {    
            DataMessage dataMsg = messages.First(m => m.Type == MessageType.READY);
            string name = Encoding.ASCII.GetString(dataMsg.Data);
            messages.TryDequeue(out dataMsg);
            return name;
        }
		public List<Player> GetPlayersOfGame()
		{
            DataMessage dataMsg = messages.First(m => m.Type == MessageType.STARTING_GAME);
            GameInfoMessage gameInfoMsg = MessagePackSerializer.Deserialize<GameInfoMessage>(dataMsg.Data);
            List<Player> players = new List<Player>();
            int numPlayers = gameInfoMsg.PlayerIds.Count;
            for (byte i = 0; i < numPlayers; i++)
            {
                if (i == gameInfoMsg.ActivePlayer)
                    players.Add(new ActivePlayer(gameInfoMsg.PlayerNames[i], gameInfoMsg.PlayerIds[i], DisplayManager.GridActivePlayer));
                else
                    players.Add(new RemotePlayer(gameInfoMsg.PlayerNames[i], gameInfoMsg.PlayerIds[i], DisplayManager.GridsOtherPlayers[(i + gameInfoMsg.ActivePlayer-1) % numPlayers], this));
            }
            return players;
		}

        public bool IsAccepted()
        {
            while(accepted == null)
            {
                Thread.Sleep(100);
            }
            return (bool) accepted;
        }
    }
}