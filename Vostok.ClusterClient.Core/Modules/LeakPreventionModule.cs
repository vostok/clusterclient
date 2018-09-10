using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Modules
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