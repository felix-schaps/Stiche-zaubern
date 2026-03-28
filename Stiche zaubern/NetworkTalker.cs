using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Stiche_Zaubern_MsgpLib;
using Windows.ApplicationModel;

namespace Stiche_zaubern
{
    public class NetworkTalker
    {
        private NetworkStream stream;
        private byte counter = 0;
        private List<DataMessage> inDelivery = new List<DataMessage>();

        public NetworkTalker(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void SendMessage(GamingMessage message)
        {
            DataMessage dataMsg = new DataMessage() 
                { Data = MessagePack.MessagePackSerializer.Serialize(message), Type = MessageType.GAMING };
            Send(dataMsg);
        }

        public void SendDeck(List<Card> mixedDeck)
        {
            byte[] data = new byte[mixedDeck.Count];
            for (int i = 0; i < mixedDeck.Count; i++)
            {
                data[i] = encodeCard(mixedDeck[i]);
            }
            DataMessage dataMsg = new DataMessage()
                { Data = data, Type = MessageType.DECK };
            Send(dataMsg);
        }

        private async Task Send(DataMessage message)
        {
            message.MessageId = counter;
            counter++;
            byte[] serialized = MessagePackSerializer.Serialize(message);
            
            inDelivery.Add(message);
            for (int i = 0; i < 3; i++)
            {
                    Send(serialized);
                    await Task.Delay(1000);
                    if (!inDelivery.Contains(message))
                    {
                        return;
                    }
            }
            throw new Exception("Sending failed");
           
        }

        private void Send(byte[] data)
        {
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            stream.Write(lengthPrefix, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private byte encodeCard(Card card)
        {
            return GameInfo.GetDeck().Encode(card);
        }

        public void SendGame(Game game)
        {
            byte numPlayers = (byte)game.Players.Count;
            List<string> strings = new List<string>(numPlayers);
            List<byte> bytes = new List<byte>(numPlayers);
            GameInfoMessage message = new GameInfoMessage();
            for (byte i = 0; i < numPlayers; i++)
            {
                Player player = game.Players[i];
                if (player is RemotePlayer remotePlayer && remotePlayer.Listener.stream == stream)
                    message.ActivePlayer = i;
                strings[i] =player.Name;
                bytes[i] = player.Id;
            }
            message.PlayerIds = bytes;
            message.PlayerNames = strings;
            DataMessage data = new DataMessage() { Type= MessageType.STARTING_GAME, Data = MessagePackSerializer.Serialize(message) };
            Send(data);
        }

        public void SendVersion()
        {
            byte[] version = Encoding.ASCII.GetBytes(Package.Current.Id.Version.ToString());
            DataMessage dataMsg = new DataMessage()
            { Data = version, Type = MessageType.CONNECTED };
            Send(dataMsg);
        }

        public void SendAcceptance(bool accept = true)
        {
            byte[] data = new byte[2];
            data[0] = (byte)MessageType.CONNECTED;
            data[1] = accept ? (byte) 1 : (byte)2;
            Send(data);
        }

        public void SendName(string text)
        {
            byte[] name = Encoding.ASCII.GetBytes(text);
            DataMessage dataMsg = new DataMessage()
            { Data = name, Type = MessageType.READY };
            Send(dataMsg);
        }

        public void SendDisconnect()
        {
            byte[] data = new byte[1];
            data[0] = (byte)MessageType.LEFT_GAME;
            Send(data);
        }

        public void SendDelivery(byte id)
        {
            byte[] data = new byte[2];
            data[0] = (byte)MessageType.DELIVERED;
            data[1] = id;
            Send(data);
        }

        public void ReceiveDelivered(byte id)
        {
            inDelivery.RemoveAll(m => m.MessageId == id);
        }
    }
}