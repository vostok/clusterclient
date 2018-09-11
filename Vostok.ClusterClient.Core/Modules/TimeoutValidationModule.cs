using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Core.Model;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class TimeoutValidationModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (context.Budget.Total < TimeSpan.Zero)
            {
                LogNegativeTimeout(context);
                return Task.FromResult(ClusterResultFactory.IncorrectArguments(context.Request));
            }

            if (context.Budget.HasExpired())
            {
                LogExpiredTimeout(context);
                return Task.FromResult(ClusterResultFactory.TimeExpired(context.Request));
            }

            return next(context);
        }

        #region Logging

        private void LogNegativeTimeout(IRequestContext context) =>
            context.Log.Error($"Request timeout has incorrect negative value: '{context.Budget.Total}'.");

        private void LogExpiredTimeout(IRequestContext context) =>
            context.Log.Error($"Request timeout expired prematurely or just was too small. Total budget = '{context.Budget.Total.ToPrettyString()}'.");

        #endregion
    }
}