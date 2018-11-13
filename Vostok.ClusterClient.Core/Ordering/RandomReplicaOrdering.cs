using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Ordering.Weighed;

namespace Vostok.Clusterclient.Core.Ordering
{
    /// <summary>
    /// Represents an ordering which returns replicas in random order.
    /// </summary>
    [PublicAPI]
    public class RandomReplicaOrdering : WeighedReplicaOrdering
    {
        public RandomReplicaOrdering()
            : base(new List<IReplicaWeightModifier>())
        {
        }
    }
}