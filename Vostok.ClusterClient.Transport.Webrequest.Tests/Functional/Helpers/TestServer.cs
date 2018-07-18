using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Vostok.Commons.Utilities;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers
{
    internal class TestServer : IDisposable
    {
        private readonly HttpListener listener;
        private volatile ReceivedRequest lastRequest;

        public TestServer()
        {
            Port = FreeTcpPortFinder.GetFreePort();
            Host = Dns.GetHostName();
            listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{Port}/");
        }

        public ReceivedRequest LastRequest => lastRequest;

        public Uri Url => new Uri($"http://{Host}:{Port}/");

        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool BufferRequestBody { get; set; } = true;

        public static TestServer StartNew(Action<HttpListenerContext> handle)
        {
            var server = new TestServer();

            server.Start(handle);

            return server;
        }

        public void Start(Action<HttpListenerContext> handle)
        {
            listener.Start();

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        var context = await listener.GetContextAsync();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(
                            () =>
                            {
                                Interlocked.Exchange(ref lastRequest, DescribeReceivedRequest(context.Request));

                                handle(context);

                                context.Response.Close();
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                });
        }

        public void Dispose()
        {
            listener.Stop();
            listener.Close();
        }

        private ReceivedRequest DescribeReceivedRequest(HttpListenerRequest request)
        {
            var query = HttpUtility.ParseQueryString(request.Url.Query);
            var receivedRequest = new ReceivedRequest
            {
                Url = request.Url,
                Method = request.HttpMethod,
                Headers = request.Headers,
                Query = query,
            };

            if (BufferRequestBody)
            {
                var bodyStream = new MemoryStream(Math.Max(4, (int) request.ContentLength64));

                request.InputStream.CopyTo(bodyStream);

                receivedRequest.Body = bodyStream.ToArray();
                receivedRequest.BodySize = bodyStream.Length;
            }
            else
            {
                try
                {
                    var buffer = new byte[16 * 1024];

                    while (true)
                    {
                        var bytesReceived = request.InputStream.Read(buffer, 0, buffer.Length);
                        if (bytesReceived == 0)
                            break;

                        receivedRequest.BodySize += bytesReceived;
                    }
                }
                catch (Exception error)
                {
                    Console.Out.WriteLine(error);
                }
            }

            return receivedRequest;
        }
    }
}