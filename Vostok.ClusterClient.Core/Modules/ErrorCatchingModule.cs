using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class ErrorCatchingModule : IRequestModule
    {
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            try
            {
                return await next(context).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return ClusterResult.Canceled(context.Request);
            }
            catch (Exception error)
            {
                context.Log.Error(error, "Unexpected failure during request execution.", error);
                return ClusterResult.UnexpectedException(context.Request);
            }
        }
    }

    internal class GlobalErrorCatchingModule : ErrorCatchingModule
    {
    }
}