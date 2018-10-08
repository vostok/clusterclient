using System;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Core.Model
{
    internal class RequestTimeBudget : TimeBudget, IRequestTimeBudget
    {
        public new static RequestTimeBudget Infinite = new RequestTimeBudget(TimeSpan.MaxValue, TimeSpan.Zero);

        private RequestTimeBudget(TimeSpan budget, TimeSpan precision)
            : base(budget, precision)
        {
        }

        public new static RequestTimeBudget StartNew(TimeSpan budget, TimeSpan precision)
        {
            var timeBudget = new RequestTimeBudget(budget, precision);
            timeBudget.Start();
            return timeBudget;
        }
    }
}