using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public enum SpecialCard
    {
        JERK = 2, FAIRY = 3, BOMB = 4, WIZARD = 5, DRAGON = 6, JUGGLER = 7
    }

    public enum CardColor
    {
        KARO, HERZ, PIK, KREUZ, SPECIAL
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

        public static Array getColors() =>  Enum.GetValues(typeof(CardColor));
    }
}