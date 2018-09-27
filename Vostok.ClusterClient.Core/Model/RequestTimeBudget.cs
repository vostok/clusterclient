using System;
using System.Diagnostics;
using Vostok.Commons.Time;

namespace Vostok.ClusterClient.Core.Model
{
    internal class RequestTimeBudget : IRequestTimeBudget
    {
        public static RequestTimeBudget Infinite = new RequestTimeBudget(TimeBudget.Infinite);
        
        private readonly TimeBudget budget;

        public static RequestTimeBudget StartNew(TimeSpan budget, TimeSpan precision) =>
            new RequestTimeBudget(budget, precision);

        public static RequestTimeBudget StartNew(TimeSpan budget) =>
            new RequestTimeBudget(budget);

        public static RequestTimeBudget StartNew(int budgetMs, int precisionMs) =>
            new RequestTimeBudget(TimeSpan.FromMilliseconds(budgetMs), TimeSpan.FromMilliseconds(precisionMs));

        public static RequestTimeBudget StartNew(int budgetMs) =>
            new RequestTimeBudget(TimeSpan.FromMilliseconds(budgetMs));

        private readonly Stopwatch watch;

        private RequestTimeBudget(TimeSpan budget, TimeSpan precision)
            : this(TimeBudget.StartNew(budget, precision))
        {
        }

        private RequestTimeBudget(TimeSpan budget)
            : this(TimeBudget.StartNew(budget))
        {
        }

        private RequestTimeBudget(TimeBudget budget)
            => this.budget = budget;

        public TimeSpan Total => budget.Total;

        public TimeSpan Precision => budget.Precision;

        public TimeSpan Remaining() => budget.Remaining;

        public TimeSpan Elapsed() => watch.Elapsed;

        public TimeSpan TryAcquire(TimeSpan neededTime) => budget.TryAcquire(neededTime);

        public bool HasExpired => budget.HasExpired;
    }
}