using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class Trick
    {
        public CardColor followColor { get; private set; }
        public int Number { get; private set; }

        private Dictionary<Player, Card> cards;

        public Trick(int number)
        {
            cards = new Dictionary<Player, Card>();
            followColor = CardColor.SPECIAL;
            Number = number;
        }

        public void addCardToTrick(Player key, Card karte)
        {
            if (cards.ContainsKey(key))
            {
                throw new Exception("Player has already lain down cards.");
            }
            if (followColor == CardColor.SPECIAL)
                followColor = karte.color;
            cards.Add(key, karte);

            if (karte.color != followColor && karte.color != CardColor.SPECIAL)
            {
                Game.getActiveRound().cannotFollowSuit(followColor, key);
            }

            key.popCard(karte);
            displayCardAtTrickBoard(key, karte);
        }
        public bool hasJuggler()
        {
            return cards.Values.Any(x => x.isJuggler());
        }
        public Card getCardOfPlayer(Player player)
        {
            if (!cards.ContainsKey(player))
                throw new Exception("Player has not lain down a card.");
            return cards[player];
        }
        public Player whoHasWonTrick()
        {
            if (cards.Count != Game.getNumPlayers())
                throw new Exception("Not all players have lain down cards.");

            Card wonCard = CardsCalculations.bestCard(cards.Values.ToList(), followColor);
            if (wonCard == null)
            {
                return null;
            }
            return cards.First(x => x.Value == wonCard).Key;
        }
        public bool beatenBy(Card karte)
        {
            if (cards.Count == 0)
                return true;

            List<Card> kartenliste = cards.Values.ToList();
            kartenliste.Add(karte);
            CardColor tempBedienfarbe = followColor;
            if (followColor == CardColor.SPECIAL)
            {
                tempBedienfarbe = karte.color;
            }
            Card won = CardsCalculations.bestCard(kartenliste, tempBedienfarbe);
            return won == karte;
        }
        public List<Card> getCards()
        {
            List<Card> cards = new List<Card>();
            foreach (Player player in Game.getPlayers())
            {
                Card card = getCardOfPlayer(player);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }
        public int getNumPlayersToLay()
        {
            return Game.getNumPlayers() - cards.Count;
        }
        public bool hasLaidDown(Player player)
        {
            return cards.ContainsKey(player);
        }

        private void displayCardAtTrickBoard(Player key, Card karte)
        {
            string name = key.display.grid.Name;
            name = name.Replace("grid_", "stich_");
            Image image = DisplayManager.GridGameBoard.Children.OfType<Image>().First(x => x.Name == name);
            image.Visibility = Windows.UI.Xaml.Visibility.Visible;
            image.Source = karte.getPictureSource();

        }
    }

}
