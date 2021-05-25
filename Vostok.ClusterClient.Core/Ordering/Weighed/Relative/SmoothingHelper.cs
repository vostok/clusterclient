using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal static class SmoothingHelper
    {
        public static double SmoothValue(
            double currentValue,
            double previousValue,
            DateTime currentTimestamp,
            DateTime previousTimestamp,
            TimeSpan timeConstant)
        {
            var timeDifference = (currentTimestamp - previousTimestamp).TotalMilliseconds;

            var alpha = 1.0 - Math.Exp(-timeDifference / timeConstant.TotalMilliseconds);

            return alpha * currentValue + (1 - alpha) * previousValue;
        }
    }
}