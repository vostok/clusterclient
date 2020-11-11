using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Sending;

namespace Vostok.Clusterclient.Core.Modules
{
    internal static class RequestModuleChainBuilder
    {
        public static IList<IRequestModule> BuildChain(IClusterClientConfiguration config, IReplicaStorageProvider storageProvider)
        {
            var responseClassifier = new ResponseClassifier();
            var requestConverter = new RequestConverter(config.Log, config.DeduplicateRequestUrl);
            var requestSender = new RequestSender(config, storageProvider, responseClassifier, requestConverter);
            var resultStatusSelector = new ClusterResultStatusSelector();

            // ReSharper disable once UseObjectOrCollectionInitializer
            var modules = new List<IRequestModule>(12 + config.Modules?.Sum(x => x.Value.Count) ?? 0);

            var addedModules = new HashSet<Type>();

            AddModule(new LeakPreventionModule());
            AddModule(new GlobalErrorCatchingModule());
            AddModule(new RequestTransformationModule(config.RequestTransforms));
            AddModule(new AuxiliaryHeadersModule());

            // -->> user-defined modules by default inserted here <<-- //

            AddModule(new LoggingModule(config.Logging.LogRequestDetails, config.Logging.LogResultDetails, config.TargetServiceName));
            AddModule(new ResponseTransformationModule(config.ResponseTransforms));
            AddModule(new ErrorCatchingModule());
            AddModule(new RequestValidationModule());

            AddModule(new TimeoutValidationModule());
            AddModule(new RequestRetryModule(config.RetryPolicy, config.RetryStrategyEx));

            // -->> adaptive throttling module <<-- //

            AddModule(new AbsoluteUrlSenderModule(responseClassifier, config.ResponseCriteria, resultStatusSelector));

            // -->> replica budgeting module <<-- //

            // -->> service-mesh module is injected before RequestExecutionModule <<-- //

            AddModule(
                new RequestExecutionModule(
                    config.ResponseSelector,
                    storageProvider,
                    requestSender,
                    resultStatusSelector,
                    config.ReplicasFilters));

            return modules;

            void AddModules(IEnumerable<IRequestModule> modulesRange)
            {
                if (modulesRange == null)
                    return;

                foreach (var module in modulesRange)
                    AddModule(module);
            }

            void AddModule(IRequestModule module)
            {
                if (config.ModulesToRemove.Contains(module.GetType()))
                    return;

                var moduleType = module.GetType();

                var isNewModule = addedModules.Add(moduleType);

                if (!isNewModule || config.Modules == null)
                {
                    modules.Add(module);
                    return;
                }

                var relatedModules = config.Modules.TryGetValue(moduleType, out var v) ? v : null;

                AddModules(relatedModules?.Before);
                modules.Add(module);
                AddModules(relatedModules?.After);
            }
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