using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    internal class HttpMethodValidationModule_Tests
    {
        private readonly HttpMethodValidationModule module = new HttpMethodValidationModule();

        [TestCase(RequestMethods.Get)]
        [TestCase(RequestMethods.Delete)]
        [TestCase(RequestMethods.Head)]
        [TestCase(RequestMethods.Options)]
        [TestCase(RequestMethods.Patch)]
        [TestCase(RequestMethods.Post)]
        [TestCase(RequestMethods.Put)]
        [TestCase(RequestMethods.Trace)]
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
            var context = new RequestContext(request, RequestParameters.Empty.WithStrategy(Strategy.Sequential1), RequestTimeBudget.Infinite, new SilentLog(), null, 0);
            var result = module.ExecuteAsync(context, c => Task.FromResult(ClusterResult.Canceled(request))).GetAwaiter().GetResult();

            return result.Status != ClusterResultStatus.IncorrectArguments;
        }
    }
}