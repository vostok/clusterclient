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
        private Weights weights;

        [SetUp]
        public void SetUp()
        {
            weights = new Weights(1.Hours());
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
            weights = new Weights(50.Milliseconds());
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
    }
}