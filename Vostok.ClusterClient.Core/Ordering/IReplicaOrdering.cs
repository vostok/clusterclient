using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;

namespace Vostok.ClusterClient.Core.Ordering
{
    /// <summary>
    /// Represent a replica ordering which will be used to determine order of replicas for request sending attempts.
    /// </summary>
    [PublicAPI]
    public interface IReplicaOrdering
    {
        /// <summary>
        /// <para>Returns given <paramref name="replicas"/> in the order they should be contacted for given <paramref name="request"/>.</para>
        /// <para>Replicas list is guaranteed to contain at least one replica.</para>
        /// <para>Implementations may use <paramref name="storageProvider"/> to fetch previously stored information about replicas.</para>
        /// <para>Implementations must satisfy following requirements:</para>
        /// <list type="bullet">
        /// <item><description>This method MUST NOT omit or duplicate any of original replicas.</description></item>
        /// <item><description>This method MUST NOT introduce any new replicas.</description></item>
        /// <item><description>This method MUST be thread-safe.</description></item>
        /// </list>
        /// </summary>
        [Pure]
        [ItemNotNull]
        IEnumerable<Uri> Order(
            [NotNull] [ItemNotNull] IList<Uri> replicas,
            [NotNull] IReplicaStorageProvider storageProvider,
            [NotNull] Request request);

        /// <summary>
        /// <para>Receives feedback via <see cref="ReplicaResult"/> obtained while sending request.</para>
        /// <para>Implementations may use this data to improve ordering quality in response to changing environment.</para>
        /// <para>Implementations may use <paramref name="storageProvider"/> to fetch/store information about replicas.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        void Learn(
            [NotNull] ReplicaResult result,
            [NotNull] IReplicaStorageProvider storageProvider);
    }
}
