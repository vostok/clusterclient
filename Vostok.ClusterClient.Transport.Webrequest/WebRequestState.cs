using System;
using System.IO;
using System.Net;
using System.Threading;
using Vostok.Commons.Time;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal class WebRequestState : IDisposable
    {
        private readonly TimeBudget timeBudget;
        private int cancellationState;
        private int disposeBarrier;

        public WebRequestState(TimeSpan timeout)
        {
            timeBudget = TimeBudget.StartNew(timeout, TimeSpan.FromMilliseconds(5));
        }

        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }

        public Stream RequestStream { get; set; }
        public Stream ResponseStream { get; set; }

        public int ConnectionAttempt { get; set; }

        public byte[] BodyBuffer { get; set; }
        public int BodyBufferLength { get; set; }

        public MemoryStream BodyStream { get; set; }
        public bool ReturnStreamDirectly { get; set; }

        public TimeSpan TimeRemaining => timeBudget.Remaining();
        public bool RequestCancelled => cancellationState > 0;

        public void Reset()
        {
            Request = null;
            Response = null;
            RequestStream = null;
            ResponseStream = null;
            BodyStream = null;
            BodyBuffer = null;
            BodyBufferLength = 0;
            ReturnStreamDirectly = false;
        }

        public void CancelRequest()
        {
            Interlocked.Exchange(ref cancellationState, 1);

            CancelRequestAttempt();
        }

        public void CancelRequestAttempt()
        {
            if (Request != null)
                try
                {
                    Request.Abort();
                }
                catch
                {
                }
        }

        public void PreventNextDispose()
        {
            Interlocked.Exchange(ref disposeBarrier, 1);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposeBarrier, 0) > 0)
                return;

            CloseRequestStream();
            CloseResponseStream();
        }

        public void CloseRequestStream()
        {
            if (RequestStream != null)
                try
                {
                    RequestStream.Close();
                }
                catch
                {
                }
                finally
                {
                    RequestStream = null;
                }
        }

        public void CloseResponseStream()
        {
            if (ResponseStream != null)
                try
                {
                    ResponseStream.Close();
                }
                catch
                {
                }
                finally
                {
                    ResponseStream = null;
                }
        }
    }
}