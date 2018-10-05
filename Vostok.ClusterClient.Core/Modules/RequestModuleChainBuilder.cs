using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Misc;
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

            // ReSharper disable once UseObjectOrCollectionInitializer
            var modules = new List<IRequestModule>(12 + 
                                                   config.Modules?.Count ?? 0 +
                                                   config.AdditionalModules?.Sum(
                                                       x => (x.Value?.Before?.Count ?? 0) +
                                                            (x.Value?.After?.Count ?? 0)) ?? 0);
            
            var addedModules = new HashSet<Type>();

            AddModule(new LeakPreventionModule());
            AddModule(new GlobalErrorCatchingModule());
            AddModule(new RequestTransformationModule(config.RequestTransforms));
            AddModule(new RequestPriorityModule());
            AddModule(new ClientApplicationIdentityModule());
            foreach (var requestModule in config.Modules ?? Enumerable.Empty<IRequestModule>())
                AddModule(requestModule);

            // -->> user-defined modules by default inserted here <<-- //

            AddModule(new LoggingModule(config.Logging.LogRequestDetails, config.Logging.LogResultDetails));
            AddModule(new ResponseTransformationModule(config.ResponseTransforms));
            AddModule(new ErrorCatchingModule());
            AddModule(new RequestValidationModule());
        
            AddModule(new TimeoutValidationModule());
            AddModule(new RequestRetryModule(config.RetryPolicy, config.RetryStrategy));

            // -->> adaptive throttling and replica budgeting modules <<-- //

            AddModule(new AbsoluteUrlSenderModule(responseClassifier, config.ResponseCriteria, resultStatusSelector));

            AddModule(
                new RequestExecutionModule(
                    config.ClusterProvider,
                    config.ReplicaOrdering,
                    config.ResponseSelector,
                    storageProvider,
                    requestSender,
                    resultStatusSelector));

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
                var moduleType = module.GetType();

                var isNewModule = addedModules.Add(moduleType);

                if (!isNewModule || config.AdditionalModules == null)
                {
                    modules.Add(module);
                    return;
                }
                
                var relatedModules = config.AdditionalModules.TryGetValue(moduleType, out var v) ? v : null;
                
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