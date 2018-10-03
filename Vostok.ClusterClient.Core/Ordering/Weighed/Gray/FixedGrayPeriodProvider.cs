using System;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Gray
{
    /// <summary>
    /// Represents a simple gray period provider which always return a given fixed value.
    /// </summary>
    [PublicAPI]
    public class FixedGrayPeriodProvider : IGrayPeriodProvider
    {
        private readonly TimeSpan grayPeriod;

        /// <param name="grayPeriod">A constant gray period which will be returned by provider.</param>
        public FixedGrayPeriodProvider(TimeSpan grayPeriod)
        {
            this.grayPeriod = grayPeriod;
        }

        /// <inheritdoc />
        public TimeSpan GetGrayPeriod() => grayPeriod;
    }
}