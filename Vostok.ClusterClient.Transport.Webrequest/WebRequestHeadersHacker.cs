using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal static class WebRequestHeadersHacker
    {
        private static readonly Action<WebHeaderCollection> Unlocker;
        private static volatile bool canUnlock;

        static WebRequestHeadersHacker()
        {
            try
            {
                var request = WebRequest.CreateHttp("http://localhost");
                var headersTypeField = request.Headers.GetType().GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
                if (headersTypeField == null)
                    return;

                var headersParameter = Expression.Parameter(typeof (WebHeaderCollection));
                var headersType = Expression.Field(headersParameter, headersTypeField);
                var headersTypeValue = Expression.Convert(Expression.Constant(0), headersTypeField.FieldType);
                var assignment = Expression.Assign(headersType, headersTypeValue);

                Unlocker = Expression.Lambda<Action<WebHeaderCollection>>(assignment, headersParameter).Compile();
            }
            catch
            {
                Unlocker = null;
            }

            canUnlock = Unlocker != null;
        }

        public static bool TryUnlockRestrictedHeaders(HttpWebRequest request, ILog log)
        {
            if (!canUnlock)
                return false;

            try
            {
                Unlocker(request.Headers);
            }
            catch (Exception error)
            {
                if (canUnlock)
                    log.Warn(error, "Failed to unlock HttpWebRequestHeaders for unsafe assignment.");

                return canUnlock = false;
            }

            return true;
        }
    }
}