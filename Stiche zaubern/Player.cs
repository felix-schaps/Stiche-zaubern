using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stiche_zaubern
{
    public abstract class Player
    {
        public string Name { get =>_playerLib.Name; set => _playerLib.Name = value; }
        public byte Id { get=> _playerLib.Id; private set => _playerLib.Id = value; }
        public int Points { get => _playerLib.Points; private set => _playerLib.Points = value; }
        public DisplayPlayer display { get; protected set; }

        private Stiche_Zaubern_MsgpLib.Player _playerLib;
        protected Player(Stiche_Zaubern_MsgpLib.Player playerLib)
        {
            _playerLib = playerLib;
        }

        public PlayerInRound getPlayerInActiveRound()
        {
            return ActiveRoundInfo.getPlayerInRound(this);
        }

        public void updatePoints()
        {
            Points += getPlayerInActiveRound().calculatePoints();
        }

        public virtual void giveCards(List<Card> cards)
        {
            giveCards(cards.ToImmutableSortedSet());
        }

        public virtual void giveCards(ImmutableSortedSet<Card> cards)
        {
            getPlayerInActiveRound().giveCards(cards);
            display.displayGivenCards(getPlayerInActiveRound().Hand);
        }

        public virtual void popCard(Card popping)
        {
            getPlayerInActiveRound().popCard(popping);
        }

        public virtual void giveJugglingCard(Card card)
        {
            getPlayerInActiveRound().giveJugglingCard(card);
        }

        public Stiche_Zaubern_MsgpLib.Player getPlayerLib()
        {
            return _playerLib;
        }


    }

}
