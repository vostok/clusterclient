using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class Weights : IWeights
    {
        private readonly RelativeWeightSettings settings;
        private volatile Dictionary<Uri, Weight> currentWeights =
            new Dictionary<Uri, Weight>();

        public Weights(RelativeWeightSettings settings) =>
            this.settings = settings;

        public Weight? Get(Uri replica) =>
            currentWeights.TryGetValue(replica, out var weight)
                ? DateTime.UtcNow - weight.Timestamp <= settings.WeightsTTL
                    ? weight
                    : (Weight?)null
                : null;

        public void Update(IReadOnlyDictionary<Uri, Weight> updatedWeights)
        {
            var newReplicas = new HashSet<Uri>(updatedWeights.Select(p => p.Key));
            var newWeights = new Dictionary<Uri, Weight>(updatedWeights.Count);
            var currentTime = DateTime.UtcNow;
            foreach (var (currentReplica, currentWeight) in currentWeights)
            {
                if (newReplicas.Contains(currentReplica))
                {
                    newWeights[currentReplica] = updatedWeights[currentReplica];
                    newReplicas.Remove(currentReplica);
                    continue;
                }

                if (currentTime - currentWeight.Timestamp < settings.WeightsTTL)
                    newWeights[currentReplica] = ApplyRegenerationIfNeed(currentWeight);
            }

            foreach (var newReplica in newReplicas)
                newWeights[newReplica] = updatedWeights[newReplica];

            currentWeights = newWeights;
        }

        public void Normalize()
        {
            var maxWeight = currentWeights.Values.Max(w => w.Value);
            var newWeights = new Dictionary<Uri, Weight>();

            foreach (var (replica, currentWeight) in currentWeights)
                newWeights[replica] = new Weight(currentWeight.Value / maxWeight, currentWeight.Timestamp);

            currentWeights = newWeights;
        }

        private Weight ApplyRegenerationIfNeed(Weight weight)
        {
            var ageMinutes = (DateTime.UtcNow - weight.Timestamp).TotalMinutes;
            ageMinutes = Math.Max(0, ageMinutes - settings.RegenerationLag.TotalMinutes);

            var regenerationAmount = Math.Max(0, ageMinutes * settings.RegenerationRatePerMinute);
            var newValue = Math.Min(1.0, weight.Value + regenerationAmount);
            return new Weight(newValue, weight.Timestamp);
        }

        public IEnumerator<KeyValuePair<Uri, Weight>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<Uri, Weight>>)currentWeights).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}