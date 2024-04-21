using System;
using System.Collections.Generic;

namespace Stiche_zaubern
{
    public class NormalDistributionSelector
    {
        private readonly Random random;

        public NormalDistributionSelector()
        {
            random = new Random();
        }

        public int selectRandomValue(List<int> values, double mean, double standardDeviation)
        {
            // Berechne die Wahrscheinlichkeiten für jeden Wert in der Liste basierend auf der Normalverteilung
            Dictionary<int, double> probabilities = new Dictionary<int, double>();
            double sumProbabilities = 0.0;

            foreach (int value in values)
            {
                double probability = NormalDistributionPDF(value, mean, standardDeviation);
                probabilities[value] = probability;
                sumProbabilities += probability;
            }
            if (sumProbabilities == 0.0)
            {
                return values[0];
            }
            // Normalisiere die Wahrscheinlichkeiten, damit sie sich zu 1 addieren
            foreach (int value in values)
            {
                probabilities[value] /= sumProbabilities;
            }

            // Wähle zufällig einen Wert basierend auf den normalisierten Wahrscheinlichkeiten aus
            double randomValue = random.NextDouble();
            double cumulativeProbability = 0.0;

            foreach (KeyValuePair<int, double> kvp in probabilities)
            {
                cumulativeProbability += kvp.Value;
                if (randomValue <= cumulativeProbability)
                {
                    return kvp.Key;
                }
            }

            // Sollte normalerweise nicht erreicht werden
            return values[0];
        }

        // PDF der Normalverteilung
        private double NormalDistributionPDF(double x, double mean, double standardDeviation)
        {
            double exponent = -0.5 * Math.Pow((x - mean) / standardDeviation, 2);
            return (1.0 / (standardDeviation * Math.Sqrt(2.0 * Math.PI))) * Math.Exp(exponent);
        }
    }

}
