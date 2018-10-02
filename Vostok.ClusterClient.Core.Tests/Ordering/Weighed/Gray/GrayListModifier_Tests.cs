using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.ClusterClient.Core.Ordering.Weighed.Gray;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Core.Tests.Ordering.Weighed.Gray
{
    [TestFixture]
    internal class GrayListModifier_Tests
    {
        private double weight;
        private Uri replica1;
        private Uri replica2;
        private IList<Uri> replicas;
        private Request request;
        private DateTime currentTime;
        private ConcurrentDictionary<Uri, DateTime> storage;

        private IReplicaStorageProvider storageProvider;
        private IGrayPeriodProvider periodProvider;
        private Func<DateTime> getCurrentTime;

        private GrayListModifier modifier;

        [SetUp]
        public void TestSetup()
        {
            weight = 1.0;
            request = Request.Get("foo/bar");
            replica1 = new Uri("http://replica1");
            storage = new ConcurrentDictionary<Uri, DateTime>();
           
            storageProvider = Substitute.For<IReplicaStorageProvider>();
            storageProvider.Obtain<DateTime>(Arg.Any<string>()).Returns(storage);
            replica2 = new Uri("http://replica2");
            replicas = new List<Uri> {replica1, replica2};
         
            periodProvider = Substitute.For<IGrayPeriodProvider>();
            periodProvider.GetGrayPeriod().Returns(5.Minutes());

            currentTime = DateTime.UtcNow;
            getCurrentTime = () => currentTime;

            modifier = new GrayListModifier(periodProvider, getCurrentTime, new ConsoleLog());
        }

        [Test]
        public void Learn_method_should_do_nothing_when_response_verdict_is_accept()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.Accept), storageProvider);

            storage.Should().BeEmpty();
        }

        [Test]
        public void Learn_method_should_do_nothing_when_response_verdict_is_dont_know()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.DontKnow), storageProvider);

            storage.Should().BeEmpty();
        }

        [Test]
        public void Learn_method_should_do_nothing_when_response_code_indicates_stream_reuse_failure()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.Reject, Responses.StreamReuseFailure), storageProvider);

            storage.Should().BeEmpty();
        }

        [Test]
        public void Learn_method_should_do_nothing_when_response_code_indicates_stream_input_failure()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.Reject, Responses.StreamInputFailure), storageProvider);

            storage.Should().BeEmpty();
        }

        [Test]
        public void Learn_method_should_put_replica_with_rejected_response_to_gray_list()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.Reject), storageProvider);

            storage.Should().Contain(replica1, currentTime);
        }

        [Test]
        public void Learn_method_should_refresh_gray_timestamp_if_a_previous_timestamp_is_already_stored_for_replica()
        {
            modifier.Learn(CreateResult(replica1, ResponseVerdict.Reject), storageProvider);

            ShiftCurrentTime(1.Minutes());

            modifier.Learn(CreateResult(replica1, ResponseVerdict.Reject), storageProvider);

            storage.Should().Contain(replica1, currentTime);
        }

        [Test]
        public void Modify_method_should_not_modify_weight_if_nothing_is_stored_for_given_replica()
        {
            storage[replica1] = currentTime;

            modifier.Modify(replica2, replicas, storageProvider, request, ref weight);

            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_method_should_not_modify_weight_if_a_stale_timestamp_is_stored_for_given_replica()
        {
            storage[replica1] = currentTime - 5.Minutes() - 1.Seconds();

            modifier.Modify(replica1, replicas, storageProvider, request, ref weight);

            weight.Should().Be(1.0);
        }

        [Test]
        public void Modify_method_should_remove_stale_timestamps()
        {
            storage[replica1] = currentTime - 5.Minutes() - 1.Seconds();

            modifier.Modify(replica1, replicas, storageProvider, request, ref weight);

            storage.Should().NotContainKey(replica1);
        }

        [Test]
        public void Modify_method_should_turn_weight_to_zero_for_recently_grayed_replicas()
        {
            storage[replica1] = currentTime - 4.Minutes();

            modifier.Modify(replica1, replicas, storageProvider, request, ref weight);

            weight.Should().Be(0.0);
        }

        [Test]
        public void Modify_method_should_keep_recent_gray_timestamps()
        {
            storage[replica1] = currentTime - 4.Minutes();

            modifier.Modify(replica1, replicas, storageProvider, request, ref weight);

            storage.Should().ContainKey(replica1);
        }

        private void ShiftCurrentTime(TimeSpan delta)
        {
            currentTime = currentTime + delta;
        }

        private static ReplicaResult CreateResult(Uri replica, ResponseVerdict verdict, Response response = null)
        {
            return new ReplicaResult(replica, response ?? Responses.Timeout, verdict, TimeSpan.Zero);
        }
    }
}
