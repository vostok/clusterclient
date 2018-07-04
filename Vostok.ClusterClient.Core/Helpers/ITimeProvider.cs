using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }
}