using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Retry
{
    public class RetryStrategyAdapter : IRetryStrategyEx
    {
        private readonly IRetryStrategy retryStrategy;

        public RetryStrategyAdapter(IRetryStrategy retryStrategy)
        {
            this.retryStrategy = retryStrategy;
        }

        public TimeSpan? GetRetryDelay(IRequestContext context, ClusterResult lastResult, int attemptsUsed)
        {
            if (attemptsUsed >= retryStrategy.AttemptsCount)
            {
                return null;
            }

            return retryStrategy.GetRetryDelay(attemptsUsed);
        }
    }
}