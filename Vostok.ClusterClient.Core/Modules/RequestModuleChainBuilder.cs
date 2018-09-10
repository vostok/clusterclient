using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Abstractions.Ordering.Storage;
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
                modules.AddRange(config.Modules.Where(x => !IsWellKnownModule(x)));

            modules.Add(new LoggingModule(config.Logging.LogPrefixEnabled, config.Logging.LogRequestDetails, config.Logging.LogResultDetails));
            modules.Add(new ResponseTransformationModule(config.ResponseTransforms));
            modules.Add(new ErrorCatchingModule());
            modules.Add(new RequestValidationModule(config.ValidateHttpMethod));
            modules.Add(new TimeoutValidationModule());
            modules.Add(new RequestRetryModule(config.RetryPolicy, config.RetryStrategy));
            var adaptiveThrottling = config.Modules?.FirstOrDefault(x => x is AdaptiveThrottlingModule);
            var replicaBudgeting = config.Modules?.FirstOrDefault(x => x is ReplicaBudgetingModule);
            
            if (adaptiveThrottling != null)
                modules.Add(adaptiveThrottling);

            if (replicaBudgeting != null)
                modules.Add(replicaBudgeting);

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
                    ? Task.FromResult(ClusterResultFactory.Canceled(ctx.Request))
                    : currentModule.ExecuteAsync(ctx, currentResult);
            }

            return result;
        }

        private static bool IsWellKnownModule(IRequestModule module)
            => module is AdaptiveThrottlingModule || module is ReplicaBudgetingModule;
    }
}