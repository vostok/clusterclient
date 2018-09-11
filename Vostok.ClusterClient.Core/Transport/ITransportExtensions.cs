using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Transport
{
    [PublicAPI]
    public static class ITransportExtensions
    {
        [Pure]
        public static bool Supports(this ITransport transport, TransportCapabilities capabilities)
        {
            return (transport.Capabilities & capabilities) == capabilities;
        }
    }
}