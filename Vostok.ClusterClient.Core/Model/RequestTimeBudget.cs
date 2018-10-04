using System;
using System.Diagnostics;
using Vostok.Commons.Time;

namespace Vostok.ClusterClient.Core.Model
{
    internal class RequestTimeBudget : IRequestTimeBudget
    {
        public static RequestTimeBudget Infinite = new RequestTimeBudget(TimeBudget.Infinite);

        private readonly TimeBudget budget;

        private RequestTimeBudget(TimeBudget budget)
            => this.budget = budget;

        public static RequestTimeBudget StartNew(TimeSpan budget, TimeSpan precision)
            => new RequestTimeBudget(TimeBudget.StartNew(budget, precision));

        public static RequestTimeBudget StartNew(TimeSpan budget) =>
            new RequestTimeBudget(TimeBudget.StartNew(budget));

        public static RequestTimeBudget StartNew(int budgetMs, int precisionMs)
            => StartNew(TimeSpan.FromMilliseconds(budgetMs), TimeSpan.FromMilliseconds(precisionMs));

        public static RequestTimeBudget StartNew(int budgetMs)
            => StartNew(TimeSpan.FromMilliseconds(budgetMs));

        public TimeSpan Total => budget.Total;

        public TimeSpan Precision => budget.Precision;

        public TimeSpan Remaining => budget.Remaining;

        public TimeSpan Elapsed => budget.Elapsed;

        public bool HasExpired => budget.HasExpired;

        public TimeSpan TryAcquire(TimeSpan neededTime) => budget.TryAcquire(neededTime);
    }
}