using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// <para>Sets up a decorator over current <see cref="ITransport"/> that retries <see cref="ResponseCode.ConnectFailure"/> responses according to <see cref="IClusterClientConfiguration.ConnectionAttempts"/>.</para>
        /// </summary>
        internal static void SetupConnectionAttempts(this IClusterClientConfiguration configuration)
        {
            if (configuration.Transport == null)
                return;

            configuration.Transport = new ConnectionAttemptsTransport(configuration.Transport, configuration.ConnectionAttempts);
        }
    }
}
