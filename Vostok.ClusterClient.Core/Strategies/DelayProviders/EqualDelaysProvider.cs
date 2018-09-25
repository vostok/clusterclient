using System;
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

        /// <summary>
        /// Initializes a new instance of <see cref="EqualDelaysProvider"/> class.
        /// </summary>
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