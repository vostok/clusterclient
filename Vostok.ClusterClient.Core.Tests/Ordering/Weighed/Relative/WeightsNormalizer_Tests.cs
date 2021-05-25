using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class WeightsNormalizer_Tests
    {
        private WeightsNormalizer weightsNormalizer;

        [SetUp]
        public void Setup()
        {
            weightsNormalizer = new WeightsNormalizer();
        }

        [Test]
        public void Should_correct_normalize_weights()
        {
            var r1 = new Uri("http://r1");
            var r2 = new Uri("http://r2");
            var r3 = new Uri("http://r3");
            var weights = new Dictionary<Uri, Weight>()
            {
                [r1] = new Weight(0.15, DateTime.UtcNow),
                [r2] = new Weight(0.25, DateTime.UtcNow),
                [r3] = new Weight(0.73, DateTime.UtcNow),
            };
            var max = weights.Values.Max(w => w.Value);

            weightsNormalizer.Normalize(weights, max);

            weights[r1].Value.Should().BeApproximately(0.205, 0.001);
            weights[r2].Value.Should().BeApproximately(0.342, 0.001);
            weights[r3].Value.Should().BeApproximately(1.0, 0.001);
        }
    }
}