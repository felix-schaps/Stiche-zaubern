using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class Deck
    {
        public List<Card> stapel;
        public Deck()
        {
            stapel = new List<Card>();
            foreach (CardColor color in CardColorExtensions.getColors())
            {
                if (color != CardColor.SPECIAL)
                {
                    for (int i = 2; i < 14; i++)
                    {
                        stapel.Add(new Card(color, i));
                    }
                }
            }
            stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.BOMB));
            stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.DRAGON));
            stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.FAIRY));
            for (int j = 1; j < 5; j++)
            {
                stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.JERK, j));
                stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.WIZARD, j));
            }
            stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.JUGGLER));
        }

        public List<Card> getMixedDeck()
        {
            List<Card> mixingStapel = new List<Card>(stapel);

            Random random = new Random();
            mixingStapel.Sort((x, y) => random.Next(-1, 2));

            return mixingStapel;

        }
    }
}
