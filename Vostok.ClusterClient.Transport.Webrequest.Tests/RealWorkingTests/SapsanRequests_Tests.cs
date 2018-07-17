using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.ClusterClient.Core;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Topology;
using Vostok.Logging.Abstractions;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.RealWorkingTests
{
    [TestFixture]
    public class SapsanRequests_Tests
    {
        private const string Guid = "679cc1e6-d855-48c3-b9e4-81e21b00629c";
        private ILog log;
        private Core.ClusterClient client;

        [SetUp]
        public void SetUp()
        {
            log = new ConsoleLog();
            client = new Core.ClusterClient(log,
                configuration =>
                {
                    configuration.Transport = new WebRequestTransport(log);
                    configuration.ClusterProvider = new FixedClusterProvider();
                });
        }

        [Test]
        public async Task Create_report()
        {
            var newReportJson = $@"{{
                ""Id"": ""{Guid}"",
                ""Type"": 2,
                ""Project"": ""Sapsan"",
                ""Title"": ""ClusterClient Test Report"",
                ""Description"": """",
                ""CreatedDate"": ""{DateTime.Now}"",
                ""Flag"": 0,
                ""ProcessedReportUrl"": ""https://api.dev.kontur/drive/v1/contents/srv/sapsan-test/files/processed/53094c1b-2072-4863-8dea-278e09915978"",
                ""Logs"": {{
                    ""1"": ""https://wst.dev.kontur/hmon/api/logs?daemonName=SapsanAgent_Sapsan_3Agents_636673230907475731&replicaNo=1&from=2018-07-16%207:34:36 AM&to=&download=false&ignoreCase=false"",
                    ""2"": ""https://wst.dev.kontur/hmon/api/logs?daemonName=SapsanAgent_Sapsan_3Agents_636673230907475731&replicaNo=2&from=2018-07-16%207:34:36 AM&to=&download=false&ignoreCase=false"",
                    ""3"": ""https://wst.dev.kontur/hmon/api/logs?daemonName=SapsanAgent_Sapsan_3Agents_636673230907475731&replicaNo=3&from=2018-07-16%207:34:36 AM&to=&download=false&ignoreCase=false"",
                    ""4"": ""https://wst.dev.kontur/hmon/api/logs?daemonName=SapsanAgent_Sapsan_3Agents_636673230907475731&replicaNo=4&from=2018-07-16%207:34:36 AM&to=&download=false&ignoreCase=false""
                }}
            }}";

            var content = new Content(Encoding.UTF8.GetBytes(newReportJson));
            var request = new Request(RequestMethods.Put, new Uri("https://sapsan.dev.kontur/api/report"), content);
            var result = await client.SendAsync(request);
            
            result.Response.IsSuccessful.Should().BeTrue();
            result.Response.Content.ToString().Should().Contain("CreateReportAsync");   //{Operation 'CreateReportAsync' succeed: ReportId=679cc1e6-d855-48c3-b9e4-81e21b00629c}
        }


        [Test]
        public void Get_reports()
        {
            var request = new Request(RequestMethods.Get, new Uri("https://sapsan.dev.kontur/api/reports"));
            var result = client.SendAsync(request).Result;

            result.Response.IsSuccessful.Should().BeTrue();
            result.Response.Content.ToString().Should().Contain(Guid);
        }

        [Test]
        public void Delete_report()
        {
            var request = new Request(RequestMethods.Delete, new Uri($"https://sapsan.dev.kontur/api/report/{Guid}"));
            var result = client.Send(request);

            result.Response.IsSuccessful.Should().BeTrue();
            result.Response.Content.ToString().Should().Contain("DeleteReportAsync");   //{Operation 'DeleteReportAsync' succeed: ReportId=679cc1e6-d855-48c3-b9e4-81e21b00629c}

            // todo(Mansiper): throws exception but should work
            result = client.Send(request);
            result.Response.IsSuccessful.Should().BeFalse();
            result.Response.Code.Should().Be(ResponseCode.NotFound);
        }

        [Test]
        public void Request_with_query_params()
        {
            var request = new Request(RequestMethods.Get, new Uri("https://translate.yandex.ru/"))
                .WithAdditionalQueryParameter("lang", "en-ru")
                .WithAdditionalQueryParameter("text", "test");
            var result = client.SendAsync(request).Result;

            result.Response.IsSuccessful.Should().BeTrue();
            result.Response.Content.ToString().Should().Contain("<body data-text=\"test\" data-lang=\"en-ru\">");
        }

        [Test]
        public void Timeout_query()
        {
            var request = new Request(RequestMethods.Get, new Uri("https://github.com"));
            var result = client.SendAsync(request, 100.Milliseconds()).Result;

            result.Response.IsSuccessful.Should().BeFalse();
            result.Response.Code.Should().Be(ResponseCode.RequestTimeout);
        }
    }
}