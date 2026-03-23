using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class ActivePlayer : Player
    {
        public ActivePlayer(string name, byte id, Grid position) : base(name, id)
        {
            display = new DisplayActivePlayer(position);
        }


    }
}
