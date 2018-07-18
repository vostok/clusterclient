using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.Commons.ThreadManagment;
using Vostok.Logging.Abstractions;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    [TestFixture]
    internal class TransportTestsBase
    {
        protected ILog Log;
        protected WebRequestTransport Transport;

        static TransportTestsBase()
        {
            ThreadPoolUtility.SetUp();
        }

        [SetUp]
        public void SetUp()
        {
            Log = new ConsoleLog();
            Transport = new WebRequestTransport(Log);
        }

        protected Task<Response> SendAsync(Request request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return Transport.SendAsync(request, timeout ?? 1.Minutes(), cancellationToken);
        }

        protected Response Send(Request request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return Transport.SendAsync(request, timeout ?? 1.Minutes(), cancellationToken).GetAwaiter().GetResult();
        }
    }
}
