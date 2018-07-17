using Vostok.ClusterClient.Core;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    public static class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Initialiazes configuration transport with a <see cref="WebRequestTransport"/> with given settings.
        /// </summary>
        public static void SetupWebRequestTransport(this IClusterClientConfiguration self, WebRequestTransportSettings settings) =>
            self.Transport = new WebRequestTransport(settings, self.Log);

        /// <summary>
        /// Initialiazes configuration transport with a <see cref="WebRequestTransport"/> with default settings.
        /// </summary>
        public static void SetupWebRequestTransport(this IClusterClientConfiguration self) =>
            self.Transport = new WebRequestTransport(self.Log);
    }
}