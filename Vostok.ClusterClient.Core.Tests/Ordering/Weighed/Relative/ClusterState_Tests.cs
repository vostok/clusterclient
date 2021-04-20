using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class ClusterState_Tests
    {
        private ClusterState clusterState;

        [SetUp]
        public void SetUp()
        {
            clusterState = new ClusterState(new RelativeWeightSettings());
        }

        [Test]
        public void Swap_should_create_new_raw_statistic()
        {
            var previousActiveStat = clusterState.CurrentStatistic;
            
            var _ = clusterState.SwapToNewRawStatistic();

            clusterState.CurrentStatistic.Should().NotBeSameAs(previousActiveStat);
        }
    }
}