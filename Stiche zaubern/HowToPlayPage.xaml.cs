using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class HowToPlayPage: Page
    {
        public HowToPlayPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
