using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class Weights : IWeights
    {
        private readonly TimeSpan weightsTtL;

        private Dictionary<Uri, Weight> currentWeights =
            new Dictionary<Uri, Weight>();

        public Weights(TimeSpan weightsTtL) =>
            this.weightsTtL = weightsTtL;

        public Weight? Get(Uri replica) =>
            currentWeights.TryGetValue(replica, out var weight)
                ? DateTime.UtcNow - weight.Timestamp <= weightsTtL
                    ? weight
                    : (Weight?)null
                : null;

        public void Update(IReadOnlyDictionary<Uri, Weight> updatedWeights)
        {
            var newReplicas = new HashSet<Uri>(updatedWeights.Select(p => p.Key));
            var newWeights = new Dictionary<Uri, Weight>(updatedWeights.Count);
            foreach (var (currentReplica, currentWeight) in currentWeights)
            {
                if (newReplicas.Contains(currentReplica))
                {
                    newWeights[currentReplica] = updatedWeights[currentReplica];
                    newReplicas.Remove(currentReplica);
                    continue;
                }

                if (DateTime.UtcNow - currentWeight.Timestamp < weightsTtL)
                    newWeights[currentReplica] = currentWeight;
            }

            foreach (var newReplica in newReplicas)
                newWeights[newReplica] = updatedWeights[newReplica];

            currentWeights = newWeights;
        }
    }
}