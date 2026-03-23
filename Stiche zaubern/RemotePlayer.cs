using Windows.UI.Xaml.Controls;
namespace Stiche_zaubern
{
    public class RemotePlayer : Player
    {
        public NetworkListener Listener {get; private set;}

        public RemotePlayer(string name, byte id, Grid position, NetworkListener listener) : base(name, id)
        {
            display = new DisplayOtherPlayer(position);
            Listener = listener;
        }
	}
}