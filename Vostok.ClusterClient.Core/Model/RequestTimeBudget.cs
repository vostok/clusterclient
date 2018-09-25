using System;
using System.Diagnostics;
using Vostok.Commons.Time;

namespace Vostok.ClusterClient.Core.Model
{
    internal class RequestTimeBudget : IRequestTimeBudget
    {
        public static readonly RequestTimeBudget Infinite = new RequestTimeBudget(TimeSpan.MaxValue);
        public static readonly RequestTimeBudget Expired = new RequestTimeBudget(TimeSpan.Zero);

        public static RequestTimeBudget StartNew(TimeSpan budget, TimeSpan precision) =>
            new RequestTimeBudget(budget, precision).Start();

        public static RequestTimeBudget StartNew(TimeSpan budget) =>
            new RequestTimeBudget(budget).Start();

        public static RequestTimeBudget StartNew(int budgetMs, int precisionMs) =>
            new RequestTimeBudget(TimeSpan.FromMilliseconds(budgetMs), TimeSpan.FromMilliseconds(precisionMs)).Start();

        public static RequestTimeBudget StartNew(int budgetMs) =>
            new RequestTimeBudget(TimeSpan.FromMilliseconds(budgetMs)).Start();

        private readonly Stopwatch watch;

        public RequestTimeBudget(TimeSpan budget, TimeSpan precision)
        {
            Total = budget;
            Precision = precision;
            watch = new Stopwatch();
        }

        public RequestTimeBudget(TimeSpan budget)
            : this(budget, TimeSpan.FromMilliseconds(5))
        {
        }

        public TimeSpan Total { get; }

        public TimeSpan Precision { get; }

        public RequestTimeBudget Start()
        {
            watch.Start();
            return this;
        }

        public TimeSpan Remaining()
        {
            var remaining = Total - watch.Elapsed;
            return remaining < Precision
                ? TimeSpan.Zero
                : remaining;
        }

        public TimeSpan Elapsed() => watch.Elapsed;

        public TimeSpan TryAcquireTime(TimeSpan neededTime) =>
            TimeSpanArithmetics.Min(neededTime, Remaining());

        public bool HasExpired() => Remaining() <= TimeSpan.Zero;
    }
}