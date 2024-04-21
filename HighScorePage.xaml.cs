using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public sealed partial class HighscorePage : Page
    {
        public HighscorePage()
        {
            this.InitializeComponent();
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
            List<string[]> savedHighscores = new HighscoreCsv().ReadHighscoresAsync();

            List<HighscoreEntry> loadedHighscores = new List<HighscoreEntry>();

            for (int i = 0; i < Math.Min(10, savedHighscores.Count); i++)
            {
                loadedHighscores.Add(new HighscoreEntry
                {
                    Placement = Int32.Parse(savedHighscores[i][0]),
                    Name = savedHighscores[i][1],
                    Points = Int32.Parse(savedHighscores[i][2]),
                    NumOpponents = Int32.Parse(savedHighscores[i][3]),
                    Type = savedHighscores[i][4],
                    PointsFirst = Int32.Parse(savedHighscores[i][5]),
                    PointsLast = Int32.Parse(savedHighscores[i][6]),
                    Version = savedHighscores[i][7]
                });
            }

            return loadedHighscores;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenuPage));
        }
    }
}
