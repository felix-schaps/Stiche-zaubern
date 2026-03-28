using System;
using System.Collections.Generic;
using System.Linq;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public enum SpecialCard
    {
        JERK = 2, FAIRY = 3, BOMB = 4, WIZARD = 5, DRAGON = 6, JUGGLER = 7
    }

    
    public static class CardColorExtensions
    {
        public static string getColorName(this CardColor color)
        {
            switch (color)
            {
                case CardColor.KARO:
                    return "Karo";
                case CardColor.HERZ:
                    return "Herz";
                case CardColor.PIK:
                    return "Pik";
                case CardColor.KREUZ:
                    return "Kreuz";
                case CardColor.SPECIAL:
                    return "Special";
                default:
                    return "Unknown";
            }
        }

        public static Array getColors()
        {
            List<CardColor> list = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();
            return list.Where(c => c != CardColor.SPECIAL).ToArray();
        }
    }
}