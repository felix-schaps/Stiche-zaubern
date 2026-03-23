using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public sealed partial class HighscorePage : Page
    {
        public HighscorePage()
        {
            InitializeComponent();
            LoadHighscore();
        }

        private void LoadHighscore()
        {
            List<HighscoreEntry> highscores = GetHighscoreData();

            if (highscores != null && highscores.Count > 0)
            {
                HighscoreListView.ItemsSource = highscores;
            }
            else
            {
                NoDataText.Visibility = Visibility.Visible;
                HighscoreListView.Visibility = Visibility.Collapsed;
            }
        }

        private List<HighscoreEntry> GetHighscoreData()
        {
            return new HighscoreManager().ReadHighscores();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
