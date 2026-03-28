using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public class ActivePlayer : Player
    {
        public ActivePlayer(Stiche_Zaubern_MsgpLib.Player playerLib, Grid position) : base(playerLib)
        {
            display = new DisplayActivePlayer(position);
        }


    }
}
