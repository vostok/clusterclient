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

            var modules = new List<IRequestModule>(12 + config.Modules?.Where(x => x.Value != null).SelectMany(x => x.Value).Count() ?? 0);
            
            modules.Add(new LeakPreventionModule());
            modules.AddRange(GetModulesAfter(RequestModule.LeakPrevention));
            
            modules.Add(new ErrorCatchingModule());
            modules.AddRange(GetModulesAfter(RequestModule.GlobalErrorHandling));
            
            modules.Add(new RequestTransformationModule(config.RequestTransforms));
            modules.AddRange(GetModulesAfter(RequestModule.RequestTransformation));
            
            modules.Add(new RequestPriorityApplicationModule());
            modules.AddRange(GetModulesAfter(RequestModule.Priority));
            
            modules.AddRange(GetModulesAfter(RequestModule.Default));
            // -->> user-defined modules by default inserted here <<-- //

            modules.Add(new LoggingModule(config.Logging.LogPrefixEnabled, config.Logging.LogRequestDetails, config.Logging.LogResultDetails));
            modules.AddRange(GetModulesAfter(RequestModule.Logging));
            
            modules.Add(new ResponseTransformationModule(config.ResponseTransforms));
            modules.AddRange(GetModulesAfter(RequestModule.ResponseTransformation));
            
            modules.Add(new ErrorCatchingModule());
            modules.AddRange(GetModulesAfter(RequestModule.RequestErrorHandling));
            
            modules.Add(new RequestValidationModule(config.ValidateHttpMethod));
            modules.AddRange(GetModulesAfter(RequestModule.RequestValidation));
            
            modules.Add(new TimeoutValidationModule());
            modules.AddRange(GetModulesAfter(RequestModule.TimeoutValidation));
            
            modules.Add(new RequestRetryModule(config.RetryPolicy, config.RetryStrategy));
            modules.AddRange(GetModulesAfter(RequestModule.Retry));
            
            // -->> adaptive throttling and replica budgeting modules <<-- //
            
            modules.Add(new AbsoluteUrlSenderModule(responseClassifier, config.ResponseCriteria, resultStatusSelector));
            modules.AddRange(GetModulesAfter(RequestModule.Sending));
            
            modules.Add(
                new RequestExecutionModule(
                    config.ClusterProvider,
                    config.ReplicaOrdering,
                    config.ResponseSelector,
                    storageProvider,
                    requestSender,
                    resultStatusSelector));
            modules.AddRange(GetModulesAfter(RequestModule.Execution));

            return modules;

            IEnumerable<IRequestModule> GetModulesAfter(RequestModule m)
            {
                if (config.Modules == null)
                    return Enumerable.Empty<IRequestModule>();
                return !config.Modules.TryGetValue(m, out var v) ? Enumerable.Empty<IRequestModule>() : v;
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
                    ? Task.FromResult(ClusterResultFactory.Canceled(ctx.Request))
                    : currentModule.ExecuteAsync(ctx, currentResult);
            }

            return result;
        }
    }
}