using System;
using System.Diagnostics;
using Vostok.ClusterClient.Core.Helpers;

namespace Vostok.ClusterClient.Core.Model
{
    internal class RequestTimeBudget : IRequestTimeBudget
    {
        public static RequestTimeBudget StartNew(TimeSpan total, TimeSpan precision) =>
            new RequestTimeBudget(total, precision);

        private readonly Stopwatch watch;

        private RequestTimeBudget(TimeSpan total, TimeSpan precision)
        {
            Total = total;
            Precision = precision;

            watch = new Stopwatch();
            watch.Start();
        }

        public TimeSpan Total { get; }

        public TimeSpan Precision { get; }

        public TimeSpan Elapsed => watch.Elapsed;

        public TimeSpan Remaining => TimeSpanExtensions.Max(TimeSpan.Zero, Total - Elapsed - Precision);

        public bool HasExpired => Remaining <= TimeSpan.Zero;
    }
}