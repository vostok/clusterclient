namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IRelativeWeightCalculator
    {
        Weight Calculate(in Statistic clusterStatistic, in Statistic replicaStatistic, in Weight previousWeight);
    }
}