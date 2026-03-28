using Windows.UI.Xaml.Controls;
namespace Stiche_zaubern
{
    public class RemotePlayer : Player
    {
        public NetworkListener Listener {get; private set;}

        public RemotePlayer(Stiche_Zaubern_MsgpLib.Player playerLib, Grid position, NetworkListener listener) : base(playerLib)
        {
            display = new DisplayOtherPlayer(position);
            Listener = listener;
        }
	}
}