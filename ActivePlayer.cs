using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class ActivePlayer : Player
    {
        public ActivePlayer(string name, int id, Grid position) : base(name, id)
        {
            base.display = new DisplayActivePlayer(position);
        }


    }
}
