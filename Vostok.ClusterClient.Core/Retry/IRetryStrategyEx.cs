using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Retry
{
    /// <summary>
    /// <inheritdoc cref="IRetryStrategy"/>
    /// </summary>
    [PublicAPI]
    public interface IRetryStrategyEx : IRetryStrategy
    {

        /// <summary>
        /// <para>Returns a retry delay before next attempt based on currently used attempts count and last seen result. Values less or equal to zero are ignored.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [Pure]
        TimeSpan GetRetryDelay(IRequestContext context, ClusterResult lastResult, int attemptsUsed);
    }
}