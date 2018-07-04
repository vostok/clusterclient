using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Misc;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Sending;

namespace Vostok.ClusterClient.Core.Modules
{
    internal static class RequestModuleChainBuilder
    {
        public static IList<IRequestModule> BuildChain(IClusterClientConfiguration config, IReplicaStorageProvider storageProvider)
        {
            var responseClassifier = new ResponseClassifier();
            var requestConverter = new RequestConverter(config.Log, config.DeduplicateRequestUrl);
            var requestSender = new RequestSender(config, storageProvider, responseClassifier, requestConverter);
            var resultStatusSelector = new ClusterResultStatusSelector();

            var modules = new List<IRequestModule>(12 + config.Modules?.Count ?? 0)
            {
                new LeakPreventionModule(),
                new ErrorCatchingModule(),
                new RequestTransformationModule(config.RequestTransforms),
                new RequestPriorityApplicationModule()
            };

            if (config.Modules != null)
                modules.AddRange(config.Modules);

            modules.Add(new LoggingModule(config.LogPrefixEnabled, config.LogRequestDetails, config.LogResultDetails));
            modules.Add(new ResponseTransformationModule(config.ResponseTransforms));
            modules.Add(new ErrorCatchingModule());
            modules.Add(new RequestValidationModule());
            modules.Add(new TimeoutValidationModule());
            modules.Add(new RequestRetryModule(config.RetryPolicy, config.RetryStrategy));

            if (config.AdaptiveThrottling != null)
                modules.Add(new AdaptiveThrottlingModule(config.AdaptiveThrottling));

            if (config.ReplicaBudgeting != null)
                modules.Add(new ReplicaBudgetingModule(config.ReplicaBudgeting));

            modules.Add(new AbsoluteUrlSenderModule(responseClassifier, config.ResponseCriteria, resultStatusSelector));
            modules.Add(
                new RequestExecutionModule(
                    config.ClusterProvider,
                    config.ReplicaOrdering,
                    config.ResponseSelector,
                    storageProvider,
                    requestSender,
                    resultStatusSelector));

            return modules;
        }

        public static Func<IRequestContext, Task<ClusterResult>> BuildChainDelegate(IList<IRequestModule> modules)
        {
            Func<IRequestContext, Task<ClusterResult>> result = ctx => throw new NotSupportedException();

            for (var i = modules.Count - 1; i >= 0; i--)
            {
                var currentModule = modules[i];
                var currentResult = result;

                result = ctx => ctx.CancellationToken.IsCancellationRequested
                    ? Task.FromResult(ClusterResult.Canceled(ctx.Request))
                    : currentModule.ExecuteAsync(ctx, currentResult);
            }

            return result;
        }
    }
}