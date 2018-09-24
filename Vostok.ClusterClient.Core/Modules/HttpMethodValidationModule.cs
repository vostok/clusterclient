using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class HttpMethodValidationModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var method = context.Request.Method;
            
            if (RequestMethods.All.Contains(method))
                return next(context);
            
            context.Log.Error($"Request HTTP method {method} is not valid.");
            return Task.FromResult(ClusterResult.IncorrectArguments(context.Request));
        }
    } 
}