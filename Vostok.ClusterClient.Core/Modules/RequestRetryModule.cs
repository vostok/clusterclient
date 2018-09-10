using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Abstractions.Retry;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Retry;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class RequestRetryModule : IRequestModule
    {
        private readonly IRetryPolicy retryPolicy;
        private readonly IRetryStrategy retryStrategy;

        public RequestRetryModule(IRetryPolicy retryPolicy, IRetryStrategy retryStrategy)
        {
            this.retryPolicy = retryPolicy;
            this.retryStrategy = retryStrategy;
        }

        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var attemptsUsed = 0;

            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var result = await next(context).ConfigureAwait(false);
                if (result.Status != ClusterResultStatus.ReplicasExhausted)
                    return result;

                if (context.Budget.HasExpired)
                    return result;

                if (context.Request.ContainsAlreadyUsedStream())
                    return result;

                if (++attemptsUsed >= retryStrategy.AttemptsCount)
                    return result;

                if (!retryPolicy.NeedToRetry(result.ReplicaResults))
                    return result;

                var retryDelay = retryStrategy.GetRetryDelay(attemptsUsed);
                if (retryDelay >= context.Budget.Remaining)
                    return result;

                context.Log.Info($"All replicas exhausted. Will retry after {retryDelay.ToPrettyString()}. Attempts used: {attemptsUsed}/{retryStrategy.AttemptsCount}.");

                if (retryDelay > TimeSpan.Zero)
                    await Task.Delay(retryDelay, context.CancellationToken).ConfigureAwait(false);

                (context as RequestContext)?.ResetReplicaResults();
            }
        }
    }
}