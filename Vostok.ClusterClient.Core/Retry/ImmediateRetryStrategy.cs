using System;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a retry strategy with fixed attempts count and a zero delay between attempts.
    /// </summary>
    public class ImmediateRetryStrategy : IRetryStrategy
    {
        public ImmediateRetryStrategy(int attemptsCount)
        {
            AttemptsCount = attemptsCount;
        }

        public int AttemptsCount { get; }

        public TimeSpan GetRetryDelay(int attemptsUsed) => TimeSpan.Zero;
    }
}