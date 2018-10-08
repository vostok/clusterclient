using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Core.Strategies.DelayProviders
{
    /// <summary>
    /// <para>Represents a delay provider which divides whole time budget by a fixed number (called division factor) and issues resulting value as a forking delay for all requests.</para>
    /// </summary>
    [PublicAPI]
    public class EqualDelaysProvider : IForkingDelaysProvider
    {
        private readonly int divisionFactor;

        public EqualDelaysProvider(int divisionFactor)
        {
            if (divisionFactor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisionFactor), "Division factor must be a positive number.");

            this.divisionFactor = divisionFactor;
        }

        /// <inheritdoc />
        public TimeSpan? GetForkingDelay(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas) =>
            budget.Total.Divide(Math.Min(divisionFactor, totalReplicas));

        /// <inheritdoc />
        public override string ToString() => "equal-" + divisionFactor;
    }
}