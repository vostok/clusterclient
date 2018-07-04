using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal class TimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime() => DateTime.UtcNow;
    }
}