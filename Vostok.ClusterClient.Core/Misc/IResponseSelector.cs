using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Misc
{
    /// <summary>
    /// <para>Selects a response which will be returned as a part of <see cref="ClusterResult"/> from given possibilities.</para>
    /// </summary>
    [PublicAPI]
    public interface IResponseSelector
    {
        /// <summary>
        /// <para>Selects a response which will be returned as a part of <see cref="ClusterResult"/> from given possibilities.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        /// <param name="request">Source request.</param>
        /// <param name="parameters">Parameters used to sent a request.</param>
        /// <param name="results">All replica results obtained during request execution.</param>
        /// <returns>Selected response or <c>null</c> if none was selected.</returns>
        [Pure, CanBeNull]
        Response Select([NotNull] Request request, [NotNull] RequestParameters parameters, [NotNull] IList<ReplicaResult> results);
    }
}
