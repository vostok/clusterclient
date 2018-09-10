using System;
using System.Diagnostics;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.Commons.Helpers;

namespace Vostok.ClusterClient.Core.Model
{
    internal class RequestTimeBudget : IRequestTimeBudget
    {
        private readonly TimeBudget budget;

        private RequestTimeBudget(TimeSpan total, TimeSpan precision)
        {
            budget = TimeBudget.StartNew(total, precision);
        }

        public static RequestTimeBudget StartNew(TimeSpan total, TimeSpan precision)
        {
            return new RequestTimeBudget(total, precision);
        }

        public TimeSpan Total => budget.Budget;

        public TimeSpan Precision => budget.Precision;

        public TimeSpan Elapsed => budget.Elapsed();

        public TimeSpan Remaining => budget.Remaining();

        public bool HasExpired => budget.HasExpired();
    }
}