using System.Collections.Generic;
using Vostok.ClusterClient.Core.Ordering.Weighed;

namespace Vostok.ClusterClient.Core.Ordering
{
    /// <summary>
    /// Represents an ordering which returns replicas in random order.
    /// </summary>
    public class RandomReplicaOrdering : WeighedReplicaOrdering
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RandomReplicaOrdering"/> class.
        /// </summary>
        public RandomReplicaOrdering()
            : base(new List<IReplicaWeightModifier>())
        {
        }
    }
}