using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IWeightsNormalizer
    {
        void Normalize(Dictionary<Uri, Weight> weights, double maxWeight);
    }
}