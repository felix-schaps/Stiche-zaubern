using System;
using MessagePack;

namespace Stiche_Zaubern_MsgpLib
{
    [MessagePackObject]
    public class HighscoreEntry
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public int ComparablePoints { get; set; }
        [Key(2)]
        public int Points { get; set; }
        [Key(3)]
        public int Placement { get; set; }
        [Key(4)]
        public int NumOpponents { get; set; } // Anzahl Gegner
        [Key(5)]
        public GameType Type { get; set; } //Art -> Computergegner, online o.ä.
        [Key(6)]
        public int PointsFirst { get; set; }
        [Key(7)]
        public int PointsLast { get; set; }
        [Key(8)]
        public DateTime Date {  get; set; }
        [Key(9)]
        public string Version { get; set; } //Version, mit der das Spiel aufgezeichnet wurde
    }
}
