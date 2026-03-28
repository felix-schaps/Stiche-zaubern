using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
            Loaded += MainMenuPage_Loaded;
        }

        // Pseudocode / Plan:
        // 1. Beim Laden der Seite asynchron prüfen, ob "SaveGame.dat" im LocalFolder existiert.
        // 2. Dazu ApplicationData.Current.LocalFolder.TryGetItemAsync("SaveGame.dat") aufrufen.
        // 3. Wenn das zurückgegebene IStorageItem != null ist, ContinueButton aktivieren, sonst deaktivieren.
        // 4. Fehler abfangen und ContinueButton deaktiviert lassen, falls ein Fehler auftritt.
        private async void MainMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckSaveGameExistsAsync();
        }

        private async Task CheckSaveGameExistsAsync()
        {
            try
            {
                var item = await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("SaveGame.dat");
                ContinueButton.IsEnabled = item != null;
            }
            catch (Exception ex)
            {
                // Optional: Fehler in OutputPane/Debugger ausgeben
                System.Diagnostics.Debug.WriteLine($"Fehler beim Prüfen der SaveGame-Datei: {ex}");
                ContinueButton.IsEnabled = false;
            }
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

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(GamePage));

            _ = new GameManager();
        }
    
    }
}
