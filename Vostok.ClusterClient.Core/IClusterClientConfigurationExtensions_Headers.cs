using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// <para>Sets up a decorator over current <see cref="ITransport"/> that enriches all requests with given <paramref name="header"/> containing request timeout in seconds in the following format: <c>s.mmm</c>.</para>
        /// </summary>
        public static void SetupRequestTimeoutHeader(this IClusterClientConfiguration configuration, string header = HeaderNames.RequestTimeout)
        {
            if (configuration.Transport == null)
                return;

            configuration.Transport = new TimeoutHeaderTransport(configuration.Transport, header);
        }

        /// <summary>
        /// <para>Sets up a module that enriches all requests with two auxiliary headers:</para>
        /// <list type="bullet">
        ///     <item><description><paramref name="priorityHeader"/> with the value of request's <see cref="RequestParameters.Priority"/></description></item>
        ///     <item><description><paramref name="identityHeader"/> with the value of configuration's <see cref="IClusterClientConfiguration.ClientApplicationName"/></description></item>
        /// </list>
        /// </summary>
        public static void SetupAuxiliaryHeaders(
            this IClusterClientConfiguration configuration, 
            string priorityHeader = HeaderNames.RequestPriority,
            string identityHeader = HeaderNames.ApplicationIdentity)
        {
            configuration.AddRequestModule(new AuxiliaryHeadersModule(priorityHeader, identityHeader));
        }
    }
}