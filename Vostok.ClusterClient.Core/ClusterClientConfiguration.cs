using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core
{
    internal class ClusterClientConfiguration : IClusterClientConfiguration
    {
        public ClusterClientConfiguration(ILog log)
        {
            Log = log;
            Modules = new Dictionary<Type, RelatedModules>();
            ModulesToRemove = new HashSet<Type>();
            
            ReplicasFilters = new List<IReplicasFilter>();
            RequestTransforms = new List<IRequestTransformMetadata>();
            ResponseTransforms = new List<IResponseTransform>();
            ResponseCriteria = new List<IResponseCriterion>();
            ReplicaStorageScope = ClusterClientDefaults.ReplicaStorageScope;

            DefaultTimeout = ClusterClientDefaults.Timeout;
            DefaultConnectionTimeout = ClusterClientDefaults.ConnectionTimeout;

            Logging = new LoggingOptions
            {
                LogRequestDetails = ClusterClientDefaults.LogRequestDetails,
                LogResultDetails = ClusterClientDefaults.LogResultDetails,
                LogReplicaRequests = ClusterClientDefaults.LogReplicaRequests,
                LogReplicaResults = ClusterClientDefaults.LogReplicaResults
            };

            ConnectionAttempts = ClusterClientDefaults.ConnectionAttempts;
            DefaultConnectionTimeout = ClusterClientDefaults.ConnectionTimeout;
            MaxReplicasUsedPerRequest = ClusterClientDefaults.MaxReplicasUsedPerRequest;

            DeduplicateRequestUrl = ClusterClientDefaults.DeduplicateRequestUrl;
        }

        public ILog Log { get; }

        public ITransport Transport { get; set; }

        public IClusterProvider ClusterProvider { get; set; }
        
        public List<IReplicasFilter> ReplicasFilters { get; set; }

        public IReplicaTransform ReplicaTransform { get; set; }

        public IReplicaOrdering ReplicaOrdering { get; set; }

        public ReplicaStorageScope ReplicaStorageScope { get; set; }

        public List<IRequestTransformMetadata> RequestTransforms { get; set; }

        public List<IResponseTransform> ResponseTransforms { get; set; }

        public List<IResponseCriterion> ResponseCriteria { get; set; }

        public IDictionary<Type, RelatedModules> Modules { get; set; }

        public ISet<Type> ModulesToRemove { get; set; }

        public IRetryPolicy RetryPolicy { get; set; }

        public IRetryStrategy RetryStrategy { get; set; }

        public IRetryStrategyEx RetryStrategyEx { get; set; }

        public IResponseSelector ResponseSelector { get; set; }

        public IRequestStrategy DefaultRequestStrategy { get; set; }

        public TimeSpan DefaultTimeout { get; set; }

        public TimeSpan? DefaultConnectionTimeout { get; set; }

        public RequestPriority? DefaultPriority { get; set; }

        public int MaxReplicasUsedPerRequest { get; set; }

        public LoggingOptions Logging { get; set; }

        public string ClientApplicationName { get; set; }

        public string TargetServiceName { get; set; }

        public string TargetEnvironment { get; set; }

        public bool DeduplicateRequestUrl { get; set; }

        public int ConnectionAttempts { get; set; }

        public bool IsValid => !Validate().Any();

        public IEnumerable<string> Validate()
        {
            if (Transport == null)
                yield return "Transport implementation is not set. It is a required part of configuration.";

            if (ClusterProvider == null)
                yield return "Cluster provider implementation is not set. It is a required part of configuration.";

            if (ResponseCriteria?.Count > 0)
            {
                var lastCriterion = ResponseCriteria.Last();
                if (!(lastCriterion is AlwaysRejectCriterion) && !(lastCriterion is AlwaysAcceptCriterion))
                    yield return $"Last response criterion must always be either an '{typeof(AlwaysRejectCriterion).Name}' or '{typeof(AlwaysAcceptCriterion).Name}'.";
            }

            if (ResponseCriteria != null && ResponseCriteria.Any(criterion => criterion == null))
                yield return "One of provided response criteria is null";

            if (RequestTransforms != null && RequestTransforms.Any(transform => transform == null))
                yield return "One of provided request transforms is null";

            if (ReplicasFilters != null && ReplicasFilters.Any(filter => filter == null))
                yield return "One of provided replica filters is null";

            if (ResponseTransforms != null && ResponseTransforms.Any(transform => transform == null))
                yield return "One of provided response transforms is null";

            if (Modules != null && Modules.SelectMany(x => x.Value.After.Concat(x.Value.Before)).Any(module => module == null))
                yield return "One of provided request modules is null";

            if (DefaultTimeout <= TimeSpan.Zero)
                yield return $"Default timeout must be positive, but was '{DefaultTimeout}'";

            if (DefaultConnectionTimeout.HasValue && DefaultConnectionTimeout <= TimeSpan.Zero)
                yield return $"Default connection timeout must be positive, but was '{DefaultConnectionTimeout}'";

            if (MaxReplicasUsedPerRequest <= 0)
                yield return $"Maximum replicas per request must be > 0, but was {MaxReplicasUsedPerRequest}";

            if (ConnectionAttempts <= 0)
                yield return $"Connection attempts per replica must be > 0, but was {ConnectionAttempts}";
        }

        public void ValidateOrDie()
        {
            if (IsValid)
                return;

            var builder = new StringBuilder();

            builder.AppendLine("There are some errors in cluster client configuration:");

            foreach (var errorMessage in Validate())
            {
                builder.Append("\t");
                builder.Append("--> ");
                builder.AppendLine(errorMessage);
            }

            throw new ClusterClientException(builder.ToString());
        }

        public void AugmentWithDefaults()
        {
            if (ReplicaOrdering == null)
                ReplicaOrdering = ClusterClientDefaults.ReplicaOrdering(Log);

            if (ResponseCriteria == null || ResponseCriteria.Count == 0)
                ResponseCriteria = ClusterClientDefaults.ResponseCriteria();

            if (RetryPolicy == null)
                RetryPolicy = ClusterClientDefaults.RetryPolicy;

            if (RetryStrategyEx == null)
            {
                RetryStrategyEx = RetryStrategy == null
                    ? ClusterClientDefaults.RetryStrategyEx
                    : new RetryStrategyAdapter(RetryStrategy);
            }

            if (ResponseSelector == null)
                ResponseSelector = ClusterClientDefaults.ResponseSelector;

            if (DefaultRequestStrategy == null)
                DefaultRequestStrategy = ClusterClientDefaults.RequestStrategy;

            if (ClientApplicationName == null)
                ClientApplicationName = ClusterClientDefaults.ClientApplicationName;
        }
    }
}