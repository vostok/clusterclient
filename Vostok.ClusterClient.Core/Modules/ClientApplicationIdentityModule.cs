using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class ClientApplicationIdentityModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (!string.IsNullOrEmpty(context.ClientApplicationName))
                context.Request = context.Request.WithHeader(HeaderNames.ApplicationIdentity, context.ClientApplicationName);

            return next(context);
        }
    }
}