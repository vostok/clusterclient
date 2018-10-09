using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class HttpMethodValidationModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var method = context.Request.Method;

            if (All.Contains(method))
                return next(context);

            context.Log.Error($"Request HTTP method '{method}' is not valid.");
            return Task.FromResult(ClusterResult.IncorrectArguments(context.Request));
        }

        /// <summary>
        /// <para>A set of valid HTTP request methods.</para>
        /// <para>Includes GET, POST, PUT, HEAD, PATCH, DELETE, OPTIONS and TRACE methods.</para>
        /// </summary>
        private static readonly HashSet<string> All = new HashSet<string>
        {
            RequestMethods.Get,
            RequestMethods.Post,
            RequestMethods.Put,
            RequestMethods.Head,
            RequestMethods.Patch,
            RequestMethods.Delete,
            RequestMethods.Options,
            RequestMethods.Trace
        };
    }
}