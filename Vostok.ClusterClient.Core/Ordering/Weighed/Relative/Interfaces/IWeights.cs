using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IWeights
    {
        Weight? Get(Uri replica);

        void Update(IReadOnlyDictionary<Uri, Weight> updatedWeights);
    }
}