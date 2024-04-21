namespace Stiche_zaubern
{
    public class HighscoreEntry
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public int Placement { get; set; } // Platz in der Highscore-Liste

        public int NumOpponents { get; set; } // Anzahl Gegner
        public string Type { get; set; } //Art -> Computergegner, online o.ä.
        public int PointsFirst { get; set; }
        public int PointsLast { get; set; }
        public string Version { get; set; } //Version, mit der das Spiel aufgezeichnet wurde
    }
}
