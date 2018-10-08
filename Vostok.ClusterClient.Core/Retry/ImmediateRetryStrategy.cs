using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Retry
{
    /// <summary>
    /// Represents a retry strategy with fixed attempts count and a zero delay between attempts.
    /// </summary>
    [PublicAPI]
    public class ImmediateRetryStrategy : IRetryStrategy
    {
        /// <param name="attemptsCount">Maximum attempts count.</param>
        public ImmediateRetryStrategy(int attemptsCount)
        {
            AttemptsCount = attemptsCount;
        }

        /// <summary>
        /// Maximum attempts count.
        /// </summary>
        public int AttemptsCount { get; }

        /// <inheritdoc />
        public TimeSpan GetRetryDelay(int attemptsUsed) => TimeSpan.Zero;
    }
}