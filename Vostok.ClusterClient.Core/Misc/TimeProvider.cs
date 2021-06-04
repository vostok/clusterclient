using System;

namespace Vostok.Clusterclient.Core.Misc
{
    internal class TimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime() =>
            DateTime.UtcNow;
    }
}