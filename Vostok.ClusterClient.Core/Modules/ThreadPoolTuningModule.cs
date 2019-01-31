using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class ThreadPoolTuningModule : IRequestModule
    {
        public static readonly ThreadPoolTuningModule Instance = new ThreadPoolTuningModule();

        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var result = await next(context).ConfigureAwait(false);

            if (result.Response.Code == ResponseCode.RequestTimeout)
            {
                ThreadPoolMonitor.ReportAndFixIfNeeded(context.Log);
            }

            return result;
        }
    }
}