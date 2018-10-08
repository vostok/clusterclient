using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class LeakPreventionModule : IRequestModule
    {
        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            LeakPreventionTransport leakTransport;

            context.Transport = leakTransport = new LeakPreventionTransport(context.Transport);

            var result = await next(context).ConfigureAwait(false);

            leakTransport.CompleteRequest(result);

            return result;
        }
    }
}