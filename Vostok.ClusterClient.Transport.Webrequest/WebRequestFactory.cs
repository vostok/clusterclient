using System;
using System.Net;
using Vostok.ClusterClient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal static class WebRequestFactory
    {
        public static HttpWebRequest Create(Request request, TimeSpan timeout, WebRequestTransportSettings settings, ILog log)
        {
            var webRequest = WebRequest.CreateHttp(request.Url);

            webRequest.Method = request.Method;

            WebRequestTuner.Tune(webRequest, timeout, settings);

            if (settings.FixNonAsciiHeaders)
                request = NonAsciiHeadersFixer.Fix(request);

            WebRequestHeadersFiller.Fill(request, webRequest, timeout, log);

            return webRequest;
        }
    }
}