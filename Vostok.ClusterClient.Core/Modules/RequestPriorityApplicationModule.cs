using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.Context;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class RequestPriorityApplicationModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var priority = context.Priority ?? FlowingContext.Get<RequestPriority?>(""); // todo(Mansiper): fix it: what string do we need?
            if (priority.HasValue)
                context.Request = context.Request.WithHeader(HeaderNames.XKonturRequestPriority, priority.Value);

            return next(context);
        }
    }
}