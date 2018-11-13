using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Gray
{
    /// <summary>
    /// A provider of gray period.
    /// </summary>
    [PublicAPI]
    public interface IGrayPeriodProvider
    {
        /// <summary>
        /// <para>Returns current gray period. See <see cref="GrayListModifier"/> for more info about gray period.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [Pure]
        TimeSpan GetGrayPeriod();
    }
}