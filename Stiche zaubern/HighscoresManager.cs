using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using Windows.ApplicationModel;
using Stiche_Zaubern_MsgpLib;

namespace Stiche_zaubern
{
    public class HighscoreManager
    {
        private readonly string filePath = "\\Highscores.dat";
        private readonly string localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        public List<HighscoreEntry> ReadHighscores()
        {
            List<HighscoreEntry> data = new List<HighscoreEntry>();

            try
            {
                byte[] bindata = File.ReadAllBytes(localFolder + filePath);
                data = MessagePackSerializer.Deserialize<List<HighscoreEntry>>(bindata);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error reading binary data: {ex.Message}");
            }

            return data;
        }

        public void WriteHighscore(string name, int points, int placement, int oppo, GameType type, int pointsFirst, int pointsLast)
        {
            List<HighscoreEntry> data = ReadHighscores();

            PackageVersion version = Package.Current.Id.Version;
            string versionString = $"{version.Major}.{version.Minor}.{version.Build}";
            DateTime dateTime = DateTime.Now;

            int comparablePoints = points * (int) Math.Sqrt((oppo + 1) ^ 3);

            HighscoreEntry newScore = new HighscoreEntry() { Name=name, ComparablePoints = comparablePoints, Points=points, Placement=placement, NumOpponents=oppo, Type=type, PointsFirst=pointsFirst, PointsLast=pointsLast, Date=dateTime, Version=versionString};
            int ins;
            for (ins = 0; ins < data.Count; ins++)
            {
                if (data[ins].ComparablePoints < comparablePoints)
                {
                        break;
                }
            }
            data.Insert(ins, newScore);
            WriteCsv(data);
        }

        private void WriteCsv(List<HighscoreEntry> data)
        {
            try
            {
                byte[] bindata = MessagePackSerializer.Serialize(data);
                File.WriteAllBytes(localFolder + filePath, bindata);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error writing binary data: {ex.Message}");
            }
        }
    }
}