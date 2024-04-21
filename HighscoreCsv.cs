using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel;

namespace Stiche_zaubern
{
    public class HighscoreCsv
    {
        private string filePath = "\\Highscores.csv";
        private string localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        public List<string[]> ReadHighscoresAsync()
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
            List<string[]> data = ReadHighscoresAsync();

            PackageVersion version = Package.Current.Id.Version;
            string versionString = $"{version.Major}.{version.Minor}.{version.Build}";


            string[] newData = { placement.ToString(), name, points.ToString(), oppo.ToString(), type, pointsFirst.ToString(), pointsLast.ToString(), versionString };
            int ins = 0;
            for (ins = 0; ins < data.Count; ins++)
            {
                int dataoutput = 0;
                if (Int32.TryParse(data[ins][2], out dataoutput))
                {
                    if (dataoutput < points)
                        break;
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