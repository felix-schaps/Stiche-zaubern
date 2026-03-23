using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(GameSelectionPage));
        }

        private void HighscoreButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(HighscorePage));
        }

        private void NetworkHost_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(NetworkHostPage));
        }

        private void NetworkClient_Click(object sender, RoutedEventArgs e)
        {
			_ = Frame.Navigate(typeof(NetworkConnectPage));
		}

        private void HowToPlay_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(HowToPlayPage));
        }
    }
}
