namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IRelativeWeightCalculator
    {
        Weight Calculate(in AggregatedStatistic clusterAggregatedStatistic, in AggregatedStatistic replicaAggregatedStatistic, in Weight previousWeight);
    }
}