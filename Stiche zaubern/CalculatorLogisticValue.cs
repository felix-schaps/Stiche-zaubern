using System;

namespace Stiche_zaubern
{
    public class CalculatorLogisticValue
    {
        private readonly double weight;
        public CalculatorLogisticValue(double weight)
        {
            this.weight = weight;
        }

        public double calc(double x)
        {
            return x < 0 || x > 1
                ? throw new ArgumentException("Invalid argument in CalculatorLogisticValue.")
                : x <= 0.5 ? Math.Pow(2 * x, weight) * 0.5 : (-0.5 * Math.Pow(2 - (2 * x), weight)) + 1;
        }

    }
}
