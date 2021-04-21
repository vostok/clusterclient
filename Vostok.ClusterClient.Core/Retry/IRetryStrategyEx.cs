using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Retry
{
    /// <summary>
    /// <para>Represents a strategy which determines cluster communication attempts count and delays between attempts.</para>
    /// <para>Note that this retry mechanism applies to whole cluster communication attempts (it only gets used when all replicas have failed to produce an <see cref="Model.ResponseVerdict.Accept"/>ed response).</para>
    /// <para>Such a retry mechanism is suitable for small clusters which can be fully temporarily unavailable during normal operation (such as leadership ensembles).</para>
    /// </summary>
    [PublicAPI]
    public interface IRetryStrategyEx
    {

        /// <summary>
        /// <para>Returns a retry delay before next attempt based on currently used attempts count and last seen result. Values less or equal to zero are ignored.</para>
        /// <para>Returning null value indicates that all attempts have been exhausted.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [Pure]
        TimeSpan? GetRetryDelay(IRequestContext context, ClusterResult lastResult, int attemptsUsed);
    }
}