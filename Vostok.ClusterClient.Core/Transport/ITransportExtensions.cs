namespace Vostok.ClusterClient.Core.Transport
{
    public static class ITransportExtensions
    {
        public static bool Supports(this ITransport transport, TransportCapabilities capabilities) =>
            (transport.Capabilities & capabilities) == capabilities;
    }
}