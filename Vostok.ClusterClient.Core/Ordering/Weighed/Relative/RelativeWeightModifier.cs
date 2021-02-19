using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    public class RelativeWeightModifier : IReplicaWeightModifier
    {
        private readonly RelativeWeightSettings settings;
        private readonly WeighingHelper weighingHelper;
        private readonly object sync = new object();
        private readonly string storageKey;

        public RelativeWeightModifier(
            string service, 
            string environment, 
            RelativeWeightSettings settings)
        {
            this.settings = settings;

            storageKey = CreateStorageKey(service, environment);
            weighingHelper = new WeighingHelper(3, settings.Sensitivity);
        }

        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var clusterState = storageProvider.ObtainGlobalValue(storageKey, CreateClusterState);
            
            ModifyWeightsIfNeed(clusterState);

            weight *= clusterState.Weights.Get(replica, settings.WeightsTTL)?.Value ?? settings.InitialWeight;
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
            var clusterState = storageProvider.ObtainGlobalValue(storageKey, CreateClusterState);

            clusterState.ActiveStatistic.Report(result);
        }

        private void ModifyWeightsIfNeed(ClusterState clusterState)
        {
            if (!NeedUpdateWeights(DateTime.UtcNow, clusterState.LastUpdateTimestamp)) return;

            lock (sync)
            {
                if (!NeedUpdateWeights(DateTime.UtcNow, clusterState.LastUpdateTimestamp)) return;

                clusterState.LastUpdateTimestamp = DateTime.UtcNow;

                ModifyWeights(clusterState.LastUpdateTimestamp, clusterState.ExchangeActiveStat(), clusterState);
            }
        }

        private void ModifyWeights(DateTime currentTimestamp, ActiveStatistic activeStat, ClusterState clusterState)
        {
            var rejectPenalty = activeStat.CalculatePenalty();
            var previousClusterStatistic = clusterState.StatisticsHistory.GetCluster();
            var smoothedClusterStatistic = activeStat.ObserveCluster(currentTimestamp, rejectPenalty, previousClusterStatistic);
            var replicasHistory = new Dictionary<Uri, Statistic>();
            var newWeights = new Dictionary<Uri, Weight>();
            
            foreach (var (replica, smoothedStatistic) in activeStat
                .ObserveReplicas(currentTimestamp, rejectPenalty, uri => clusterState.StatisticsHistory.Get(uri)))
            {
                var previousWeight = clusterState.Weights.Get(replica, settings.WeightsTTL) ??
                                     new Weight(settings.InitialWeight, currentTimestamp - settings.WeightUpdatePeriod);

                var newReplicaWeight = CalculateWeight(smoothedClusterStatistic, smoothedStatistic, previousWeight);

                replicasHistory.Add(replica, smoothedStatistic);
                newWeights.Add(replica, newReplicaWeight);
            }

            clusterState.StatisticsHistory.Update(smoothedClusterStatistic, replicasHistory);
            clusterState.Weights.Update(newWeights);
        }

        private Weight CalculateWeight(in Statistic clusterStatistic, in Statistic replicaStatic, in Weight previousWeight)
        {
            var rowWeight = weighingHelper
                .ComputeWeight(replicaStatic.Mean, replicaStatic.StdDev, clusterStatistic.Mean, clusterStatistic.StdDev);
            var newWeight = Math.Max(settings.MinWeight, rowWeight);
            var smc = newWeight > previousWeight.Value 
                ? settings.WeightsRaiseSmoothingConstant 
                : settings.WeightsDownSmoothingConstant;
            var smoothedWeight = SmoothingHelper
                .SmoothValue(newWeight, previousWeight.Value, clusterStatistic.Timestamp, previousWeight.Timestamp, smc);
            return new Weight(smoothedWeight, replicaStatic.Timestamp);
        }

        private bool NeedUpdateWeights(DateTime currentTimestamp, DateTime previousTimestamp) =>
            currentTimestamp - previousTimestamp > settings.WeightUpdatePeriod;

        private string CreateStorageKey(string service, string environment) =>
            $"{nameof(RelativeWeightModifier)}_{environment}_{service}";

        private ClusterState CreateClusterState() =>
            new ClusterState(settings);
    }
}