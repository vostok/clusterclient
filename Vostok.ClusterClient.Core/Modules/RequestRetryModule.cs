using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class RequestRetryModule : IRequestModule
    {
        private readonly IRetryPolicy retryPolicy;
        private readonly IRetryStrategyEx retryStrategy;

        public RequestRetryModule(IRetryPolicy retryPolicy, IRetryStrategyEx retryStrategy)
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
                if (result.Status != ClusterResultStatus.ReplicasExhausted &&
                    result.Status != ClusterResultStatus.ReplicasNotFound)
                    return result;

                if (context.Budget.HasExpired)
                    return result;

                if (context.Request.ContainsAlreadyUsedStream())
                    return result;

                if (!retryPolicy.NeedToRetry(context.Request, context.Parameters, result.ReplicaResults))
                    return result;

                attemptsUsed++;
                var retryDelay = retryStrategy.GetRetryDelay(context, result, attemptsUsed);
                if (!retryDelay.HasValue || retryDelay.Value >= context.Budget.Remaining)
                    return result;

                context.Log.Info("Could not obtain an acceptable response from cluster. Will retry after {RetryDelay}. Attempts used: {AttemptsUsed}.", retryDelay.Value.ToPrettyString(), attemptsUsed);

                if (retryDelay > TimeSpan.Zero)
                    await Task.Delay(retryDelay.Value, context.CancellationToken).ConfigureAwait(false);

                context.ResetReplicaResults();
            }
        }
    }
}