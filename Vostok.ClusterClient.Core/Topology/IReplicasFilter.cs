using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Topology
{
    /// <summary>
    /// Represents a replica filter which will be used to configure replica filter rules based on given request parameters.
    /// </summary>
    [PublicAPI]
    public interface IReplicasFilter
    {
        /// <summary>
        /// <para>Returns filtered <paramref name="replicas"/> based on the given <paramref name="requestContext"/> rules.</para>
        /// <para>May return an empty list if all replicas have been filtered.</para>
        /// <para>Implementations should take care to cache the result for optimal performance.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [Pure]
        [ItemNotNull]
        IEnumerable<Uri> Filter([ItemNotNull] IEnumerable<Uri> replicas, [NotNull] IRequestContext requestContext);
    }
}