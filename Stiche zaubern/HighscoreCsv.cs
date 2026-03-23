using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;

namespace Stiche_zaubern
{
    [Obsolete("Use HighscoreManager")]
    public class HighscoreCsv
    {
        private readonly string filePath = "\\Highscores.csv";
        private readonly string localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        public List<string[]> ReadHighscores()
        {
            List<string[]> data = new List<string[]>();

            try
            {
                using (StreamReader parser = new StreamReader(localFolder + filePath))
                {

                    while (!parser.EndOfStream)
                    {
                        string line = parser.ReadLine();
                        string[] fields = line.Split(';');
                        data.Add(fields);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error reading CSV: {ex.Message}");
            }

            return data;
        }

        public void WriteHighscore(string name, int points, int placement, int oppo, string type, int pointsFirst, int pointsLast)
        {
            List<string[]> data = ReadHighscores();

            PackageVersion version = Package.Current.Id.Version;
            string versionString = $"{version.Major}.{version.Minor}.{version.Build}";


            string[] newData = { placement.ToString(), name, points.ToString(), oppo.ToString(), type, pointsFirst.ToString(), pointsLast.ToString(), versionString };
            int ins;
            for (ins = 0; ins < data.Count; ins++)
            {
                if (int.TryParse(data[ins][2], out int dataoutput))
                {
                    if (dataoutput < points)
                    {
                        break;
                    }
                }
                else
                {
                    throw new Exception("CSV file corrupt.");

                }

            }
            data.Insert(ins, newData);
            WriteCsv(data);
        }

        private void WriteCsv(List<string[]> data)
        {
            try
            {
                using (StreamWriter writer = File.CreateText(localFolder + filePath))
                {
                    foreach (string[] row in data)
                    {
                        string line = string.Join(";", row);
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error writing CSV: {ex.Message}");
            }
        }
    }
}