using System;
using System.Net;
using System.Net.Security;
using Vostok.Commons.Utilities;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal static class WebRequestTuner
    {
        private static readonly bool IsMono = RuntimeDetector.IsMono;

        static WebRequestTuner()
        {
            if (!IsMono)
            {
                HttpWebRequest.DefaultMaximumErrorResponseLength = -1;
                HttpWebRequest.DefaultMaximumResponseHeadersLength = -1;

                ServicePointManager.CheckCertificateRevocationList = false;
                ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;
            }
        }

        public static void Tune(HttpWebRequest request, TimeSpan timeout, WebRequestTransportSettings settings)
        {
            request.ConnectionGroupName = settings.ConnectionGroupName;
            request.Expect = null;
            request.KeepAlive = true;
            request.Pipelined = settings.Pipelined;
            request.Proxy = settings.Proxy;
            request.AllowAutoRedirect = settings.AllowAutoRedirect;
            request.AllowWriteStreamBuffering = false;
            request.AllowReadStreamBuffering = false;
            request.AuthenticationLevel = AuthenticationLevel.None;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.ConnectionLimit = settings.MaxConnectionsPerEndpoint;
            request.ServicePoint.UseNagleAlgorithm = false;

            if (!IsMono)
                request.ServicePoint.ReceiveBufferSize = 16*1024;

            var timeoutInMilliseconds = Math.Max(1, (int)timeout.TotalMilliseconds);
            request.Timeout = timeoutInMilliseconds;
            request.ReadWriteTimeout = timeoutInMilliseconds;
        }
    }
}