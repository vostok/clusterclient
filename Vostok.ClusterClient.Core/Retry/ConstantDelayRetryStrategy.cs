using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Retry
{
    /// <summary>
    /// Represents a retry strategy with fixed attempts count and a constant delay between attempts.
    /// </summary>
    [PublicAPI]
    public class ConstantDelayRetryStrategy : IRetryStrategy
    {
        /// <param name="attemptsCount">Maximum attempts count.</param>
        /// <param name="retryDelay">A retry delay which constant strategy will be return.</param>
        public ConstantDelayRetryStrategy(int attemptsCount, TimeSpan retryDelay)
        {
            AttemptsCount = attemptsCount;
            RetryDelay = retryDelay;
        }

        /// <summary>
        /// Maximum attempts count.
        /// </summary>
        public int AttemptsCount { get; }

        /// <summary>
        /// A retry delay which constant strategy returns.
        /// </summary>
        public TimeSpan RetryDelay { get; }

        /// <inheritdoc />
        public TimeSpan GetRetryDelay(int attemptsUsed) => RetryDelay;
    }
}