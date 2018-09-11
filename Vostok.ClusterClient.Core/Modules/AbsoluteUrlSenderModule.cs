using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Misc;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class AbsoluteUrlSenderModule : IRequestModule
    {
        private readonly IResponseClassifier responseClassifier;
        private readonly IList<IResponseCriterion> responseCriteria;
        private readonly IClusterResultStatusSelector resultStatusSelector;

        public AbsoluteUrlSenderModule(
            IResponseClassifier responseClassifier,
            IList<IResponseCriterion> responseCriteria,
            IClusterResultStatusSelector resultStatusSelector)
        {
            this.responseClassifier = responseClassifier;
            this.responseCriteria = responseCriteria;
            this.resultStatusSelector = resultStatusSelector;
        }

        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (!context.Request.Url.IsAbsoluteUri)
                return next(context);

            return SendAsync(context);
        }

        private async Task<ClusterResult> SendAsync(IRequestContext context)
        {
            var elapsedBefore = context.Budget.Elapsed();
            var response = await context.Transport.SendAsync(context.Request, context.Budget.Remaining(), context.CancellationToken).ConfigureAwait(false);
            if (response.Code == ResponseCode.Canceled)
                return ClusterResultFactory.Canceled(context.Request);

            var responseVerdict = responseClassifier.Decide(response, responseCriteria);
            var replicaResult = new ReplicaResult(context.Request.Url, response, responseVerdict, context.Budget.Elapsed() - elapsedBefore);
            var replicaResults = new[] {replicaResult};
            var resultStatus = resultStatusSelector.Select(replicaResults, context.Budget);

            return new ClusterResult(resultStatus, replicaResults, response, context.Request);
        }
    }
}