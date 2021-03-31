using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal static class WeighingHelper
    {
        public static double ComputeWeight(
            double replicaAverage,
            double replicaStdDev,
            double globalAverage,
            double globalStdDev,
            double sensitivity)
        {
            // http://homework.uoregon.edu/pub/class/es202/ztest.html

            var stdDev = Math.Sqrt(replicaStdDev * replicaStdDev + globalStdDev * globalStdDev);

            var weight = ComputeCDF(globalAverage, replicaAverage, stdDev);

            return ApplySensitivity(weight, sensitivity);
        }

        public static double ComputeCDF(double x, double mean, double stdDev)
        {
            if (stdDev <= double.Epsilon)
                return mean > x ? 0.0d : (mean < x ? 1.0d : 0.5d);

            var y = (x - mean) / stdDev;

            return 1.0 / (1.0 + Math.Exp(-y * (1.5976 + 0.070566 * y * y)));
        }

        private static double ApplySensitivity(double weight, double sensitivity) =>
            sensitivity < double.Epsilon ? 1 : Math.Pow(weight, sensitivity);
    }
}