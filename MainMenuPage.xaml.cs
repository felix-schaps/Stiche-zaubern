using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            this.InitializeComponent();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GameSelectionPage));
        }

        private void HighscoreButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HighscorePage));
        }
    }
}
