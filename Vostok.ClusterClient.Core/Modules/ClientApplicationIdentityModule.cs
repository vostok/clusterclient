using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class ClientApplicationIdentityModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (!string.IsNullOrEmpty(context.ClientApplicationName))
                context.Request = context.Request.WithHeader(HeaderNames.ClientApplication, context.ClientApplicationName);

            return next(context);
        }
    }
}