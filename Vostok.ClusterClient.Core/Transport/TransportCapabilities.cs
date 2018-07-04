using System;

namespace Vostok.ClusterClient.Core.Transport
{
    /// <summary>
    /// Represents a set of optional capabilities one can expect to find in transport implementations.
    /// </summary>
    [Flags]
    public enum TransportCapabilities
    {
        None = 0,
        RequestStreaming = 1 << 0,
        ResponseStreaming = 1 << 1
    }
}