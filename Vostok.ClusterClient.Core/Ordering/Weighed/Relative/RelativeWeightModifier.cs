using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
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
        private readonly WeighingHelper weighingHelper;
        private readonly object sync = new object();
        private readonly string storageKey;
        private readonly ILog log;

        public RelativeWeightModifier(
            string service, 
            string environment, 
            RelativeWeightSettings settings, 
            ILog log)
        {
            this.settings = settings;
            this.log = (log ?? new SilentLog()).ForContext<RelativeWeightModifier>();

            storageKey = CreateStorageKey(service, environment);
            weighingHelper = new WeighingHelper(3, settings.Sensitivity);
        }

        /// <inheritdoc />
        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var clusterState = storageProvider.ObtainGlobalValue(storageKey, CreateClusterState);
            
            ModifyWeightsIfNeed(clusterState);

            weight *= clusterState.Weights.Get(replica)?.Value ?? settings.InitialWeight;
        }

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider) =>
            storageProvider.ObtainGlobalValue(storageKey, CreateClusterState).CurrentStatistic.Report(result);

        private void ModifyWeightsIfNeed(ClusterState clusterState)
        {
            if (!NeedUpdateWeights(DateTime.UtcNow, clusterState.LastUpdateTimestamp)) return;

            lock (sync)
            {
                if (!NeedUpdateWeights(DateTime.UtcNow, clusterState.LastUpdateTimestamp)) return;

                ModifyWeights(clusterState.ExchangeStatistic(DateTime.UtcNow), clusterState);
            }
        }

        private void ModifyWeights(StatisticSnapshot statisticSnapshot, ClusterState clusterState)
        {
            var newWeights = new Dictionary<Uri, Weight>(statisticSnapshot.Replicas.Count);
            foreach (var (replica, replicaStatistic) in statisticSnapshot.Replicas)
            {
                var previousWeight = clusterState.Weights.Get(replica) ??
                                     new Weight(settings.InitialWeight, clusterState.LastUpdateTimestamp - settings.WeightUpdatePeriod);
                var newReplicaWeight = CalculateWeight(statisticSnapshot.Cluster, replicaStatistic, previousWeight);

                newWeights.Add(replica, newReplicaWeight);
            }
            clusterState.Weights.Update(newWeights);

            LogWeights(newWeights);
        }

        private Weight CalculateWeight(in Statistic clusterStatistic, in Statistic replicaStatistic, in Weight previousWeight)
        {
            var newWeight = Math.Max(settings.MinWeight, weighingHelper
                .ComputeWeight(replicaStatistic.Mean, replicaStatistic.StdDev, clusterStatistic.Mean, clusterStatistic.StdDev));
            var smc = newWeight > previousWeight.Value 
                ? settings.WeightsRaiseSmoothingConstant 
                : settings.WeightsDownSmoothingConstant;
            var smoothedWeight = SmoothingHelper
                .SmoothValue(newWeight, previousWeight.Value, clusterStatistic.Timestamp, previousWeight.Timestamp, smc);
            return new Weight(smoothedWeight, replicaStatistic.Timestamp);
        }

        private bool NeedUpdateWeights(in DateTime currentTimestamp, in DateTime previousTimestamp) =>
            currentTimestamp - previousTimestamp > settings.WeightUpdatePeriod;

        private string CreateStorageKey(string service, string environment) =>
            $"{nameof(RelativeWeightModifier)}_{environment}_{service}";

        private ClusterState CreateClusterState() =>
            new ClusterState(settings);

        private void LogWeights(IReadOnlyDictionary<Uri, Weight> weights)
        {
            if (!log.IsEnabledForInfo()) return;

            var newWeightsLog = new StringBuilder($"New weights:{Environment.NewLine}");
            foreach (var (replica, weight) in weights)
                newWeightsLog.AppendLine($"{replica}: {weight}");
            log.Info(newWeightsLog.ToString());
        }
    }
}