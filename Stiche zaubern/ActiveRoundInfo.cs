using System;
using System.Collections.Generic;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class ActiveRoundInfo
    {
        private readonly GameRound activeRound;
        private static ActiveRoundInfo Instance;

        public ActiveRoundInfo(GameRound activeRound)
        {
            this.activeRound = activeRound;
            Instance = this;
        }
        public static CardColor getTrumpColor()
        {
            return Instance.activeRound.TrumpColor;
        }
        public static Card getTrumpCard()
        {
            return Instance.activeRound.TrumpCard;
        }
        public static int getNumberRound()
        {
            return Instance.activeRound.NumberRound;
        }
        public static RoundMode getRoundMode()
        {
            return Instance.activeRound.RoundMode;
        }
        public static PlayerInRound getPlayerInRound(Player player)
        {
            return Instance.activeRound.GetPlayerInRound(player);
        }
        public static GuessHelper getActiveGuessHelper()
        {
            return Instance.activeRound.GuessHelper;
        }
        public static int getNumCardsToLay()
        {
            return Instance.activeRound.GetNumCardsToLay();
        }
        public static List<PlayerInRound> getListOfKnownCannotFollow(CardColor color)
        {
            return Instance.activeRound.GetListOfKnownCannotFollow(color);
        }
        public static List<Card> getPlayedCards()
        {
            return Instance.activeRound.GetPlayedCards();
        }
        public static List<PlayerInRound> getOtherPlayers(PlayerInRound playerInRound)
        {
            List<PlayerInRound> list = getPlayersInRound();
            return !list.Remove(playerInRound) ? throw new Exception("Player not part of this game.") : list;
        }

        public static List<PlayerInRound> getPlayersInRound()
        {
            return Instance.activeRound.GetPlayersInRound();
        }
    }

    
}
