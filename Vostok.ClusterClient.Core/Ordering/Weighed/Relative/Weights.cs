using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class Weights : IWeights
    {
        private readonly TimeSpan weightsTTL;

        public Weights(TimeSpan weightsTTL) =>
            this.weightsTTL = weightsTTL;

        private readonly Dictionary<Uri, Weight> weights =
            new Dictionary<Uri, Weight>();

        public Weight? Get(Uri replica) =>
            weights.TryGetValue(replica, out var weight)
                ? DateTime.UtcNow - weight.Timestamp <= weightsTTL 
                    ? weight 
                    : (Weight?)null 
                : null;

        public void Update(IReadOnlyDictionary<Uri, Weight> newWeights)
        {
            var newReplicas = new HashSet<Uri>(newWeights.Select(p => p.Key));
            foreach (var (replica, weight) in weights.Select(p => p).ToArray())
            {
                if (newReplicas.Contains(replica))
                {
                    weights[replica] = newWeights[replica];
                    newReplicas.Remove(replica);
                }
                if (DateTime.UtcNow - weight.Timestamp > weightsTTL)
                    weights.Remove(replica);
            }
            foreach (var replica in newReplicas)
                weights[replica] = newWeights[replica];
        }
    }
}