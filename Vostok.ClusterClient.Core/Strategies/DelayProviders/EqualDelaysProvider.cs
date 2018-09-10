using System;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.ClusterClient.Core.Model;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.ClusterClient.Core.Strategies.DelayProviders
{
    /// <summary>
    /// <para>Represents a delay provider which divides whole time budget by a fixed number (called division factor) and issues resulting value as a forking delay for all requests.</para>
    /// </summary>
    public class EqualDelaysProvider : IForkingDelaysProvider
    {
        private readonly int divisionFactor;

        public EqualDelaysProvider(int divisionFactor)
        {
            if (divisionFactor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisionFactor), "Division factor must be a positive number.");

            this.divisionFactor = divisionFactor;
        }

        public TimeSpan? GetForkingDelay(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas) =>
            budget.Total.Divide(Math.Min(divisionFactor, totalReplicas));

        public override string ToString() => "equal-" + divisionFactor;
    }
}