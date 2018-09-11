using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
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
                return ClusterResultFactory.Canceled(context.Request);
            }
            catch (Exception error)
            {
                context.Log.Error(error, "Unexpected failure during request execution.", error);
                return ClusterResultFactory.UnexpectedException(context.Request);
            }
        }
    }
}