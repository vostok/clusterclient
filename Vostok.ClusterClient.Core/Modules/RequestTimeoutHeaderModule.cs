using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class RequestTimeoutHeaderModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            context.Transport = new RequestTimeoutTransport(context.Transport);
            
            return next(context);
        }
    }
}