namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IStatisticHistory
    {
        ClusterStatistic Get();

        void Update(ClusterStatistic snapshot);
    }
}