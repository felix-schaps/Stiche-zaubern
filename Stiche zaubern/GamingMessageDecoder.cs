using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class GamingMessageDecoder {
        public readonly GamingMessage GamingMessage;
        public RoundMode GetRoundMode() { return GamingMessage.RoundMode; }

        public Player GetPlayer() { return GameInfo.GetPlayers().First(player => player.id == GamingMessage.PlayerId); }
        public object GetArg()
        {
            RoundMode roundMode = GetRoundMode();
            switch (roundMode)
            {
                case RoundMode.TRICKING:
                case RoundMode.JUGGLING:
                    return GameInfo.GetDeck().Decode(GamingMessage.Arg);
                case RoundMode.GUESSING:
                    return GamingMessage.Arg;
                case RoundMode.CHOOSING_TRUMP:
                    return (CardColor)GamingMessage.Arg;
            }
            return null;
        }
        public GamingMessageDecoder(GamingMessage gamingMessage){
            GamingMessage = gamingMessage;
        }

        public static GamingMessageDecoder CreateTrickingMessage(Player player, Card card)
        {
            GamingMessage message = new GamingMessage()
            { RoundMode = RoundMode.TRICKING, PlayerId = player.id, Arg = GameInfo.GetDeck().Encode(card) };
            return new GamingMessageDecoder(message);
        }
        public static GamingMessageDecoder CreateJugglingMessage(Player player, Card card)
        {
            GamingMessage message = new GamingMessage()
            { RoundMode = RoundMode.JUGGLING, PlayerId = player.id, Arg = GameInfo.GetDeck().Encode(card) };
            return new GamingMessageDecoder(message);
        }
        public static GamingMessageDecoder CreateGuessingMessage(Player player, int guess)
        {
            GamingMessage message = new GamingMessage()
            { RoundMode = RoundMode.GUESSING, PlayerId = player.id, Arg = (byte) guess };
            return new GamingMessageDecoder(message);
        }

        public static GamingMessageDecoder CreateChoosingTrumpMessage(Player player, CardColor color)
        {
            GamingMessage message = new GamingMessage()
            { RoundMode = RoundMode.TRICKING, PlayerId = player.id, Arg = (byte) color };
            return new GamingMessageDecoder(message);
        }

    }
}