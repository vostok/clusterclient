using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class WeighingHelper
    {
        private readonly double stdDevRatioCap;
        private readonly double sensitivity;

        public WeighingHelper(
            double stdDevRatioCap,
            double sensitivity)
        {
            this.stdDevRatioCap = stdDevRatioCap;
            this.sensitivity = sensitivity;
        }

        public double ComputeWeight(
            double replicaAverage,
            double replicaStdDev,
            double globalAverage,
            double globalStdDev)
        {
            replicaStdDev = Math.Min(replicaStdDev, globalStdDev * stdDevRatioCap);

            var weight = ComputeCDF(globalAverage, replicaAverage, replicaStdDev);

            return Math.Pow(weight, sensitivity);
        }

        public static double ComputeCDF(double x, double mean, double stdDev)
        {
            if (stdDev <= double.Epsilon)
                return mean > x ? 0.0d : (mean < x ? 1.0d : 0.5d);

            var y = (x - mean) / stdDev;

            return 1.0 / (1.0 + Math.Exp(-y * (1.5976 + 0.070566 * y * y)));
        }
    }
}