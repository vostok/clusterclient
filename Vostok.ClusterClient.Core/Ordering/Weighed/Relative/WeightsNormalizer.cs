using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class WeightsNormalizer : IWeightsNormalizer
    {
        public void Normalize(Dictionary<Uri, Weight> weights, double maxWeight)
        {
            if (weights.Count == 0) return;

            foreach (var (replica, currentWeight) in weights.ToArray())
            {
                var normalizedWeight = currentWeight.Value / maxWeight;

                weights[replica] = new Weight(normalizedWeight, currentWeight.Timestamp);
            }
        }
    }
}