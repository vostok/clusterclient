using System.Collections.Generic;
using System.Threading;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Abstractions.Strategies;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class RequestContext : IRequestContext
    {
        private readonly object resultsLock = new object();
        private List<ReplicaResult> results;

        public RequestContext(
            Request request,
            IRequestStrategy strategy,
            IRequestTimeBudget budget,
            ILog log,
            ITransport transport,
            CancellationToken cancellationToken,
            RequestPriority? priority,
            int maximumReplicasToUse)
        {
            Request = request;
            Strategy = strategy;
            Budget = budget;
            Log = log;
            Transport = transport;
            Priority = priority;
            CancellationToken = cancellationToken;
            MaximumReplicasToUse = maximumReplicasToUse;

            ResetReplicaResults();
        }

        public Request Request { get; set; }

        public IRequestStrategy Strategy { get; set; }

        public IRequestTimeBudget Budget { get; }

        public ILog Log { get; }

        public ITransport Transport { get; set; }

        public CancellationToken CancellationToken { get; }

        public RequestPriority? Priority { get; }

        public int MaximumReplicasToUse { get; set; }

        public void SetReplicaResult(ReplicaResult result)
        {
            lock (resultsLock)
            {
                if (results == null)
                    return;

                for (var i = 0; i < results.Count; i++)
                    if (results[i].Replica.Equals(result.Replica))
                    {
                        results[i] = result;
                        return;
                    }

                results.Add(result);
            }
        }

        public List<ReplicaResult> FreezeReplicaResults()
        {
            lock (resultsLock)
            {
                var currentResults = results;
                results = null;
                return currentResults;
            }
        }

        public void ResetReplicaResults()
        {
            lock (resultsLock)
                results = new List<ReplicaResult>(2);
        }
    }
}