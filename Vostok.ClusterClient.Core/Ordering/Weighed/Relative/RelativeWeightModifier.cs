using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    /// <summary>
    /// <para>Represents a weight modifier that periodically calculates the weight of a replica, which characterizes its quality relative to others in the cluster.</para>
    /// <para>Replica weight is the probability that a replica will respond in a time less than or equal to the average response time across the cluster.</para>
    /// <para>For replicas whose responses are <see cref="ResponseVerdict.Reject"/> or <see cref="ResponseVerdict.DontKnow"/> a <see cref="RelativeWeightSettings.PenaltyMultiplier"/> will be applied.</para>
    /// </summary>
    [PublicAPI]
    public class RelativeWeightModifier : IReplicaWeightModifier
    {
        private readonly RelativeWeightSettings settings;
        private readonly double minWeight;
        private readonly double initialWeight;
        private readonly double maxWeight;
        private readonly object sync = new object();
        private readonly string storageKey;
        private readonly ILog log;

        public RelativeWeightModifier(
            RelativeWeightSettings settings,
            string service,
            string environment,
            double minWeight = ClusterClientDefaults.MinimumReplicaWeight,
            double initialWeight = ClusterClientDefaults.InitialReplicaWeight,
            double maxWeight = ClusterClientDefaults.MaximumReplicaWeight,
            ILog log = null)
        {
            this.settings = settings;
            this.minWeight = minWeight;
            this.initialWeight = initialWeight;
            this.maxWeight = maxWeight;
            this.log = (log ?? new SilentLog()).ForContext<RelativeWeightModifier>();

            storageKey = CreateStorageKey(service, environment);
        }

        /// <inheritdoc />
        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var clusterState = storageProvider.ObtainGlobalValue(storageKey, CreateClusterState);
            
            ModifyWeightsIfNeed(clusterState);

            weight *= EnforceWeightLimits(clusterState.Weights.Get(replica));
        }

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider) =>
            storageProvider.ObtainGlobalValue(storageKey, CreateClusterState).CurrentStatistic.Report(result);

        private void ModifyWeightsIfNeed(ClusterState clusterState)
        {
            var needUpdateWeights = NeedUpdateWeights(DateTime.UtcNow, clusterState.LastUpdateTimestamp);
            if (!needUpdateWeights || !clusterState.IsUpdatingNow.TrySetTrue())
                return;

            var statisticSnapshot = clusterState.ExchangeStatistic(DateTime.UtcNow);
               
            ModifyWeights(statisticSnapshot, clusterState.Weights, clusterState.LastUpdateTimestamp);
            
            clusterState.IsUpdatingNow.Value = false;
        }

        private void ModifyWeights(StatisticSnapshot statisticSnapshot, IWeights weights, DateTime weightsLastUpdateTime)
        {
            var newWeights = new Dictionary<Uri, Weight>(statisticSnapshot.Replicas.Count);
            foreach (var (replica, replicaStatistic) in statisticSnapshot.Replicas)
            {
                var previousWeight = weights.Get(replica) ??
                                     new Weight(settings.InitialWeight, weightsLastUpdateTime - settings.WeightUpdatePeriod);
                var newReplicaWeight = CalculateWeight(statisticSnapshot.Cluster, replicaStatistic, previousWeight);

                newWeights.Add(replica, newReplicaWeight);
            }
            
            weights.Update(newWeights);
            weights.Normalize();
            
            LogWeights(weights);
        }

        private Weight CalculateWeight(in Statistic clusterStatistic, in Statistic replicaStatistic, in Weight previousWeight)
        {
            var newWeight = Math.Max(settings.MinWeight, WeighingHelper
                .ComputeWeight(replicaStatistic.Mean, replicaStatistic.StdDev, clusterStatistic.Mean, clusterStatistic.StdDev, settings.Sensitivity));
            var smoothingConstant = newWeight > previousWeight.Value 
                ? settings.WeightsRaiseSmoothingConstant 
                : settings.WeightsDownSmoothingConstant;
            var smoothedWeight = SmoothingHelper
                .SmoothValue(newWeight, previousWeight.Value, clusterStatistic.Timestamp, previousWeight.Timestamp, smoothingConstant);
            return new Weight(smoothedWeight, replicaStatistic.Timestamp);
        }

        private double EnforceWeightLimits(Weight? weight)
        {
            return !weight.HasValue ? 
                initialWeight : 
                Math.Max(minWeight, weight.Value.Value * maxWeight);
        }

        private bool NeedUpdateWeights(in DateTime currentTimestamp, in DateTime previousTimestamp) =>
            currentTimestamp - previousTimestamp > settings.WeightUpdatePeriod;

        private string CreateStorageKey(string service, string environment) =>
            $"{nameof(RelativeWeightModifier)}_{environment}_{service}";

        private ClusterState CreateClusterState() =>
            new ClusterState(settings);

        private void LogWeights(IEnumerable<KeyValuePair<Uri, Weight>> weights)
        {
            if (!log.IsEnabledForInfo()) return;

            var newWeightsLog = new StringBuilder($"Weights:{Environment.NewLine}");
            foreach (var (replica, weight) in weights)
                newWeightsLog.AppendLine($"{replica}: {weight}");
            log.Info(newWeightsLog.ToString());
        }
    }
}