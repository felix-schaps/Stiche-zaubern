using System;
using System.Collections.Generic;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class Deck
    {
        public static Deck InitializieAndGetInstance()
        {
            return new Deck();
        }

        private readonly List<Card> _stapel;

        private Deck()
        {
            _stapel = new List<Card>();
            foreach (CardColor color in CardColorExtensions.getColors())
            {
                for (int i = 2; i < 14; i++)
                {
                    _stapel.Add(new Card(color, i));
                }
            }
            _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.BOMB));
            _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.DRAGON));
            _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.FAIRY));
            for (int j = 1; j < 5; j++)
            {
                _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.JERK, j));
                _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.WIZARD, j));
            }
            _stapel.Add(new Card(CardColor.SPECIAL, (int)SpecialCard.JUGGLER));

            DeckException.Check(_stapel.Count < byte.MaxValue);
        }

        public List<Card> GetMixedDeck()
        {
            List<Card> mixingStapel = new List<Card>(_stapel);

            Random random = new Random();
            mixingStapel.Sort((x, y) => random.Next(-1, 2));

            return mixingStapel;

        }

        public int GetNumCards()
            { return _stapel.Count; }

        public Card Decode(byte id)
        {
            return _stapel[id];
        }

        public byte Encode(Card card)
        {
            return (byte) _stapel.IndexOf(card);
        }


    }
}
