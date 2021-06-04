using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal static class WeighingHelper
    {
        public static double ComputeWeightByLatency(
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

        public static double ComputeWeightByStatuses(double replicaTotalCount, double replicaErrorFraction, double clusterTotalCount, double clusterErrorFraction, double sensitivity)
        {
            // https://stats.stackexchange.com/a/113607

            var p1 = Round(clusterErrorFraction);
            var n1 = clusterTotalCount;

            var p2 = Round(replicaErrorFraction);
            var n2 = replicaTotalCount;

            var phat = (n1 * p1 + n2 * p2) / (n1 + n2);

            var stdDev = Math.Sqrt(phat * (1 - phat) * (1.0 / n1 + 1.0 / n2));

            var weight = ComputeCDF(p1, p2, stdDev);

            weight = ApplySensitivity(weight, sensitivity);
            
            return weight;
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

        private static double Round(double value) =>
            Math.Round(value, 5, MidpointRounding.AwayFromZero);

    }
}