using System.Collections.Generic;

namespace Stiche_zaubern
{
    public abstract class Player
    {
        public string name { get; private set; }
        public byte id { get; private set; }
        public int points { get; private set; }
        public DisplayPlayer display { get; protected set; }
        protected Player(string name, byte id)
        {
            this.name = name;
            points = 0;
            this.id = id;
        }

        public PlayerInRound getPlayerInActiveRound()
        {
            return ActiveRoundInfo.getPlayerInRound(this);
        }

        public void updatePoints()
        {
            points += getPlayerInActiveRound().calculatePoints();
        }

        public virtual void giveCards(List<Card> cards)
        {
            giveCards(new SortedSet<Card>(cards));
        }

        public virtual void giveCards(SortedSet<Card> cards)
        {
            getPlayerInActiveRound().giveCards(cards);
            display.displayGivenCards(getPlayerInActiveRound().hand);
        }

        public virtual void giveTrick(Trick stich)
        {
            getPlayerInActiveRound().giveTrick(stich);
        }

        public virtual void popCard(Card popping)
        {
            getPlayerInActiveRound().popCard(popping);
        }

        public virtual void giveJugglingCard(Card card)
        {
            getPlayerInActiveRound().giveJugglingCard(card);
        }
    }

}
