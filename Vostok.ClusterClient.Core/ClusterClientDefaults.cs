using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Misc;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Ordering;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Retry;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Ordering.Weighed;
using Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core
{
    /// <summary>
    /// A class with default values of ClusterClient settings
    /// </summary>
    [PublicAPI]
    public static class ClusterClientDefaults
    {
        /// <summary>
        /// The default duration of full health damage decay. See <see cref="AdaptiveHealthWithLinearDecay"/> for details.
        /// </summary>
        public static readonly TimeSpan AdaptiveHealthDamageDecayDuration = TimeSpan.FromMinutes(10);

        #region WeighedReplicaOrdering

        /// <summary>
        /// The default initial replica weight used by <see cref="WeighedReplicaOrdering"/>.
        /// </summary>
        public const double InitialReplicaWeight = 1.0;
        /// <summary>
        /// The default minimum replica weight used by <see cref="WeighedReplicaOrdering"/>.
        /// </summary>
        public const double MinimumReplicaWeight = 0.0;
        /// <summary>
        /// The default maximum replica weight used by <see cref="WeighedReplicaOrdering"/>.
        /// </summary>
        public const double MaximumReplicaWeight = 10.0;

        #endregion

        #region AdaptiveHealthWithLinearDecay

        /// <summary>
        /// The default value of <see cref="AdaptiveHealthWithLinearDecay.MinimumHealthValue"/>.
        /// </summary>
        public const double AdaptiveHealthMinimumValue = 0.001;

        /// <summary>
        /// The default value of <see cref="AdaptiveHealthWithLinearDecay.UpMultiplier"/>.
        /// </summary>
        public const double AdaptiveHealthUpMultiplier = 1.5;

        /// <summary>
        /// The default value of <see cref="AdaptiveHealthWithLinearDecay.DownMultiplier"/>.
        /// </summary>
        public const double AdaptiveHealthDownMultiplier = 0.5;

        #endregion

        #region AdaptiveThrottlingOptions

        /// <summary>
        /// The default value of <see cref="AdaptiveThrottlingOptions.MinimumRequests"/>.
        /// </summary>
        public const int AdaptiveThrottlingMinimumRequests = 30;

        /// <summary>
        /// The default value of <see cref="AdaptiveThrottlingOptions.MinutesToTrack"/>.
        /// </summary>
        public const int AdaptiveThrottlingMinutesToTrack = 2;

        /// <summary>
        /// The default value of <see cref="AdaptiveThrottlingOptions.CriticalRatio"/>.
        /// </summary>
        public const double AdaptiveThrottlingCriticalRatio = 2.0;

        /// <summary>
        /// The default value of <see cref="AdaptiveThrottlingOptions.MaximumRejectProbability"/>.
        /// </summary>
        public const double AdaptiveThrottlingRejectProbabilityCap = 0.8;

        #endregion

        #region ReplicaBudgetingOptions

        /// <summary>
        /// The default value of <see cref="ReplicaBudgetingOptions.MinimumRequests"/>.
        /// </summary>
        public const int ReplicaBudgetingMinimumRequests = 30;

        /// <summary>
        /// The default value of <see cref="ReplicaBudgetingOptions.MinutesToTrack"/>.
        /// </summary>
        public const int ReplicaBudgetingMinutesToTrack = 2;

        /// <summary>
        /// The default value of <see cref="ReplicaBudgetingOptions.CriticalRatio"/>.
        /// </summary>
        public const double ReplicaBudgetingCriticalRatio = 1.2;

        #endregion

        #region LoggingOptions

        /// <summary>
        /// The default value of <see cref="LoggingOptions.LogRequestDetails"/>.
        /// </summary>
        public const bool LogRequestDetails = true;

        /// <summary>
        /// The default value of <see cref="LoggingOptions.LogResultDetails"/>.
        /// </summary>
        public const bool LogResultDetails = true;

        /// <summary>
        /// The default value of <see cref="LoggingOptions.LogReplicaRequests"/>.
        /// </summary>
        public const bool LogReplicaRequests = true;

        /// <summary>
        /// The default value of <see cref="LoggingOptions.LogReplicaResults"/>.
        /// </summary>
        public const bool LogReplicaResults = true;

        #endregion

        #region IClusterClientConfiguration

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.DeduplicateRequestUrl"/>.
        /// </summary>
        public const bool DeduplicateRequestUrl = false;

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.MaxReplicasUsedPerRequest"/>.
        /// </summary>
        public const int MaxReplicasUsedPerRequest = 3;

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.ReplicaStorageScope"/>.
        /// </summary>
        public static readonly ReplicaStorageScope ReplicaStorageScope = ReplicaStorageScope.Process;

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.RetryPolicy"/>.
        /// </summary>
        public static readonly IRetryPolicy RetryPolicy = new NeverRetryPolicy();

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.RetryStrategy"/>.
        /// </summary>
        public static readonly IRetryStrategy RetryStrategy = new ConstantDelayRetryStrategy(1, TimeSpan.Zero);

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.ResponseSelector"/>.
        /// </summary>
        public static readonly IResponseSelector ResponseSelector = new LastAcceptedResponseSelector();

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/>.
        /// </summary>
        public static readonly IRequestStrategy RequestStrategy = Strategy.Forking3;

        /// <summary>
        /// The default value of <see cref="IClusterClientConfiguration.DefaultTimeout"/>.
        /// </summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        /// <returns>The default value of <see cref="IClusterClientConfiguration.ResponseCriteria"/>.</returns>
        public static List<IResponseCriterion> ResponseCriteria()
        {
            return new List<IResponseCriterion>
            {
                new AcceptNonRetriableCriterion(),
                new RejectNetworkErrorsCriterion(),
                new RejectServerErrorsCriterion(),
                new RejectThrottlingErrorsCriterion(),
                new RejectUnknownErrorsCriterion(),
                new RejectStreamingErrorsCriterion(),
                new AlwaysAcceptCriterion()
            };
        }

        /// <returns>The default value of <see cref="IClusterClientConfiguration.ReplicaOrdering"/>.</returns>
        public static IReplicaOrdering ReplicaOrdering(ILog log)
        {
            var builder = new WeighedReplicaOrderingBuilder(log);

            builder.AddAdaptiveHealthModifierWithLinearDecay(AdaptiveHealthDamageDecayDuration);

            return builder.Build();
        }

        #endregion
    }
}