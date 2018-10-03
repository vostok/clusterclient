using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Transport
{
    /// <summary>
    /// A set of extensions for <see cref="ITransport"/>.
    /// </summary>
    [PublicAPI]
    public static class TransportExtensions
    {
        /// <summary>
        /// Check that <paramref name="transport"/> supports provided <paramref name="capabilities"/>.
        /// </summary>
        /// <param name="transport">A <see cref="ITransport"/> instance</param>.
        /// <param name="capabilities">A set of <see cref="TransportCapabilities"/> flags</param>.
        /// <returns></returns>
        [Pure]
        public static bool Supports(this ITransport transport, TransportCapabilities capabilities)
        {
            return (transport.Capabilities & capabilities) == capabilities;
        }
    }
}