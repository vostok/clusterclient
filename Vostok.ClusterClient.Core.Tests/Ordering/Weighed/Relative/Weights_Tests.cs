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
            weights = new Weights();
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
                weights.Get(p.Key, 5.Seconds()).Should().Be(p.Value);
        }

        [Test]
        public void Should_return_null_if_ttl_expired()
        {
            var replica = new Uri("http://replica2");
            var newWeights = new Dictionary<Uri, Weight>()
            {
                [replica] = new Weight(0.7, DateTime.UtcNow - 100.Milliseconds()),
            };
            
            weights.Update(newWeights);
            Thread.Sleep(100);

            weights.Get(replica, 50.Milliseconds()).Should().BeNull();
        }

        [Test]
        public void Should_return_null_if_not_present()
        {
            weights.Get(new Uri("http://r1"), 5.Seconds()).Should().BeNull();
        }
    }
}