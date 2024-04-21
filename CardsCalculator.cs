using System;
using System.Collections.Generic;
using System.Linq;

namespace Stiche_zaubern
{
    public class CardsCalculations
    {
        public static void seperateJerks(List<Card> cards)
        {
            if (cards.All(x => x.isJerk() || x.isFairy()))
                return;
            cards.RemoveAll(x => x.isJerk() || x.isFairy());
        }

        public static Card bestCard(List<Card> kartenliste, CardColor followSuite)
        {
            if (kartenliste == null)
                throw new Exception("Illegal call.");

            if (kartenliste.Any(x => x.isBomb()))
            {
                return null;
            }

            if (kartenliste.Any(x => x.isDragon()) && !kartenliste.Any(x => x.isFairy()))
            {
                return kartenliste.First(x => x.isDragon());
            }

            if (kartenliste.Any(x => x.isDragon()) && kartenliste.Any(x => x.isFairy()))
            {
                return kartenliste.First(x => x.isFairy());
            }

            if (kartenliste.Any(x => x.isWizard()))
            {
                return kartenliste.First(x => x.isWizard());
            }

            CardColor trumpfFarbe = Game.getActiveRound().TrumpColor;
            if (trumpfFarbe != CardColor.SPECIAL && trumpfFarbe != followSuite)
            {
                if (kartenliste.Any(x => x.color == trumpfFarbe))
                {
                    int maxValue = kartenliste.Where(x => x.color == trumpfFarbe).Max(x => x.wert);
                    return kartenliste.First(x => x.color == trumpfFarbe && x.wert == maxValue);
                }
            }

            int maxVal = kartenliste.Where(x => x.color == followSuite).Max(x => x.wert);
            if (maxVal < 8)
            {
                if (kartenliste.Any(x => x.isJuggler()))
                {
                    return kartenliste.First(x => x.isJuggler());
                }

                if (kartenliste.All(x => x.isJerk() || x.isFairy()))
                {
                    return kartenliste.First(x => x.isJerk());
                }

            }
            return kartenliste.First(x => x.color == followSuite && x.wert == maxVal);
        }

        public static Dictionary<Card, AICard> filterDictionary(Dictionary<Card, AICard> origin, List<Card> filter)
        {
            return origin.Where(x => filter.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        public static int getNumFools(List<Card> cards)
        {
            return cards.Count(k => k.isFairy() || k.isJerk() || k.isBomb());
        }

        public static int getNumWizardsOrDragons(List<Card> cards)
        {
            return cards.Count(k => k.isWizard() || k.isDragon());
        }
    }
}
