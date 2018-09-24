using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    public class HttpMethodValidationModule_Tests
    {
        private HttpMethodValidationModule module = new HttpMethodValidationModule();

        [TestCaseSource(typeof(RequestMethods), nameof(RequestMethods.All))]
        public void Validation_should_pass_if_request_has_supported_method(string method)
        {
            var request = new Request(method, new Uri("http://localhost"));

            IsValid(request).Should().BeTrue();
        }
        [Test]
        public void Validation_should_fail_if_request_has_unsupported_method()
        {
            var request = new Request("WHATEVER", new Uri("http://localhost"));

            IsValid(request).Should().BeFalse();
        }

        private bool IsValid(Request request)
        {
            var context = new RequestContext(request, Strategy.Sequential1, RequestTimeBudget.Infinite, new SilentLog(), null, null, 0);
            var result = module.ExecuteAsync(context, c => Task.FromResult(ClusterResult.Canceled(request))).GetAwaiter().GetResult();

            return result.Status != ClusterResultStatus.IncorrectArguments;
        }
    }
}