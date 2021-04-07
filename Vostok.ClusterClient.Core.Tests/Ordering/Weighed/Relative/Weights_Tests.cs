using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    public class Weights_Tests
    {
        private RelativeWeightSettings settings;
        private Weights weights;

        [SetUp]
        public void SetUp()
        {
            settings = new RelativeWeightSettings()
            {
                WeightsTTL = 1.Hours()
            };
            weights = new Weights(settings);
        }

        [Test]
        public void Should_add_new_weights()
        {
            var newWeights = new Dictionary<Uri, Weight>()
            {
                [new Uri("http://replica1")] = new Weight(0.5, DateTime.UtcNow),
                [new Uri("http://replica2")] = new Weight(0.7, DateTime.UtcNow + 5.Seconds()),
                [new Uri("http://replica3")] = new Weight(0.1, DateTime.UtcNow + 10.Seconds())
            };
            
            weights.Update(newWeights);

            foreach (var p in newWeights)
                weights.Get(p.Key).Should().Be(p.Value);
        }

        [Test]
        public void Should_correct_update_existing_weights()
        {
            var replica1 = new Uri("http://replica1");
            var replica2 = new Uri("http://replica2");
            var replica3 = new Uri("http://replica3");
            var newWeights = new Dictionary<Uri, Weight>()
            {
                [replica1] = new Weight(0.5, DateTime.UtcNow),
                [replica2] = new Weight(0.7, DateTime.UtcNow - 5.Seconds()),
                [replica3] = new Weight(0.1, DateTime.UtcNow - 10.Seconds())
            };

            weights.Update(newWeights);

            foreach (var p in newWeights)
                weights.Get(p.Key).Should().Be(p.Value);
            
            var newWeights2 = new Dictionary<Uri, Weight>()
            {
                [replica2] = new Weight(11, DateTime.UtcNow)
            };

            weights.Update(newWeights2);

            weights.Get(replica1).Should().Be(newWeights[replica1]);
            weights.Get(replica2).Should().Be(newWeights2[replica2]);
            weights.Get(replica3).Should().Be(newWeights[replica3]);
        }

        [Test]
        public void Should_return_null_if_ttl_expired()
        {
            settings.WeightsTTL = 50.Milliseconds();
            var replica = new Uri("http://replica2");
            var newWeights = new Dictionary<Uri, Weight>()
            {
                [replica] = new Weight(0.7, DateTime.UtcNow - 100.Milliseconds()),
            };
            
            weights.Update(newWeights);
            Thread.Sleep(100);

            weights.Get(replica).Should().BeNull();
        }

        [Test]
        public void Should_return_null_if_not_present()
        {
            weights.Get(new Uri("http://r1")).Should().BeNull();
        }

        [Test]
        public void Should_correct_regenerate_weights_on_update()
        {
            settings.WeightsTTL = 5.Minutes();
            settings.RegenerationLag = 1.Minutes();
            settings.RegenerationRatePerMinute = 0.1;
            var (replica1, weight1) = (new Uri("http://replica1"), new Weight(0.5, DateTime.UtcNow - 4.Minutes()));
            var (replica2, weight2) = (new Uri("http://replica2"), new Weight(0.1, DateTime.UtcNow - 2.Minutes()));
            var (replica3, weight3) = (new Uri("http://replica3"), new Weight(0.1, DateTime.UtcNow - 3.Minutes()));
            var (replica4, weight4) = (new Uri("http://replica4"), new Weight(0.1, DateTime.UtcNow - 30.Seconds()));
            var (replica5, weight5) = (new Uri("http://replica5"), new Weight(0.98, DateTime.UtcNow - 2.Minutes()));
            var currentWeights = new Dictionary<Uri, Weight>()
            {
                [replica1] = weight1,
                [replica2] = weight2,
                [replica3] = weight3,
                [replica4] = weight4,
                [replica5] = weight5
            };
            weights.Update(currentWeights);

            weights.Update(new Dictionary<Uri, Weight>()
            {
                [replica1] = new Weight(0.8, DateTime.UtcNow)
            });

            var weightForReplica1 = weights.Get(replica1);
            weightForReplica1.Should().NotBeNull();
            weightForReplica1.Value.Value.Should().Be(0.8);

            var weightForReplica2 = weights.Get(replica2);
            weightForReplica2.Should().NotBeNull();
            weightForReplica2.Value.Value.Should().BeApproximately(0.2, 0.001);
            weightForReplica2.Value.Timestamp.Should().Be(weight2.Timestamp);

            var weightForReplica3 = weights.Get(replica3);
            weightForReplica3.Should().NotBeNull();
            weightForReplica3.Value.Value.Should().BeApproximately(0.3, 0.001);
            weightForReplica3.Value.Timestamp.Should().Be(weight3.Timestamp);

            var weightForReplica4 = weights.Get(replica4);
            weightForReplica4.Should().NotBeNull();
            weightForReplica4.Value.Should().Be(weight4);

            var weightForReplica5 = weights.Get(replica5);
            weightForReplica5.Should().NotBeNull();
            weightForReplica5.Value.Value.Should().BeApproximately(1.0, 0.001);
            weightForReplica5.Value.Timestamp.Should().Be(weight5.Timestamp);
        }
    }
}