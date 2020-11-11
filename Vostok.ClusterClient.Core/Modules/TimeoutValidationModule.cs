using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class TimeoutValidationModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (context.Budget.Total < TimeSpan.Zero)
            {
                LogNegativeTimeout(context);
                return Task.FromResult(ClusterResult.IncorrectArguments(context.Request));
            }

            if (context.Budget.Total.TotalMilliseconds > int.MaxValue)
            {
                LogTooBigTimeout(context);
                return Task.FromResult(ClusterResult.IncorrectArguments(context.Request));
            }

            if (context.Budget.HasExpired)
            {
                LogExpiredTimeout(context);
                return Task.FromResult(ClusterResult.TimeExpired(context.Request));
            }

            return next(context);
        }

        #region Logging

        private void LogNegativeTimeout(IRequestContext context) =>
            context.Log.Error("Request timeout has incorrect negative value: '{Timeout}'.", context.Budget.Total);

        private void LogTooBigTimeout(IRequestContext context) =>
            context.Log.Error("Request timeout has incorrect big value: '{Timeout}' > '{MaxValue}'.", context.Budget.Total.Milliseconds, int.MaxValue);

        private void LogExpiredTimeout(IRequestContext context) =>
            context.Log.Warn("Request timeout expired prematurely or just was too small. Total budget = '{Timeout}'.", context.Budget.Total.ToPrettyString());

        #endregion
    }
}