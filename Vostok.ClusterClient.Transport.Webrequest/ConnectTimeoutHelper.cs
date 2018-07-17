using System;
using System.Linq.Expressions;
using System.Net;
using Vostok.Commons.Utilities;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal static class ConnectTimeoutHelper
    {
        private static readonly object Sync = new object();
        private static Func<HttpWebRequest, bool> isSocketConnected;

        public static bool IsSocketConnected(HttpWebRequest request, ILog log)
        {
            Initialize(log);

            if (!CanCheckSocket)
                return true;

            try
            {
                return isSocketConnected(request);
            }
            catch (Exception error)
            {
                CanCheckSocket = false;

                WrapLog(log).Error(error, "Failed to check socket connection");
            }

            return true;
        }

        public static bool CanCheckSocket { get; private set; } = true;

        private static void Initialize(ILog log)
        {
            if (isSocketConnected != null || !CanCheckSocket)
                return;

            Exception savedError = null;

            lock (Sync)
            {
                if (isSocketConnected != null || !CanCheckSocket)
                    return;

                try
                {
                    if (RuntimeDetector.IsDotNetFramework)
                        isSocketConnected = BuildSocketConnectedChecker();
                    else
                    {
                        isSocketConnected = _ => true;
                        CanCheckSocket = false;
                    }
                }
                catch (Exception error)
                {
                    CanCheckSocket = false;
                    savedError = error;
                }
            }

            if (savedError != null)
                WrapLog(log).Error(savedError, "Failed to build connection checker lambda");
        }

        /// <summary>
        /// Builds the following lambda:
        /// (HttpWebRequest request) => request._SubmitWriteStream != null && request._SubmitWriteStream.InternalSocket != null && request._SubmitWriteStream.InternalSocket.Connected
        /// </summary>
        private static Func<HttpWebRequest, bool> BuildSocketConnectedChecker()
        {
            var request = Expression.Parameter(typeof (HttpWebRequest));

            var stream = Expression.Field(request, "_SubmitWriteStream");
            var socket = Expression.Property(stream, "InternalSocket");
            var isConnected = Expression.Property(socket, "Connected");

            var body = Expression.AndAlso(
                Expression.ReferenceNotEqual(stream, Expression.Constant(null)),
                Expression.AndAlso(
                    Expression.ReferenceNotEqual(socket, Expression.Constant(null)),
                    isConnected));

            return Expression.Lambda<Func<HttpWebRequest, bool>>(body, request).Compile();
        }

        private static ILog WrapLog(ILog log)
        {
            // todo(Mansiper): make wraper with default prefix
            return log.WithContextualPrefix();
            // return log.WithPrefix(typeof (ConnectTimeoutHelper).Name);
        }
    }
}