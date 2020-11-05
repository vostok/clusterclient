using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.ReplicaFilter
{
    /// <summary>
    /// Represents a replica filter which will be used to configure replica filter rules based on given request parameters.
    /// </summary>
    [PublicAPI]
    public interface IReplicaFilter
    {
        [Pure]
        [CanBeNull]
        [ItemNotNull]
        IList<Uri> Filter(IList<Uri> replicas, IRequestContext requestContext);
    }
}