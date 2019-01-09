using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class LoggingModule : IRequestModule
    {
        private readonly bool logRequests;
        private readonly bool logResults;

        public LoggingModule(bool logRequests, bool logResults)
        {
            this.logRequests = logRequests;
            this.logResults = logResults;
        }

        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (logRequests)
                LogRequestDetails(context);

            var result = await next(context).ConfigureAwait(false);

            if (logResults)
            {
                if (result.Status == ClusterResultStatus.Success)
                    LogSuccessfulResult(context, result);
                else
                    LogFailedResult(context, result);
            }

            return result;
        }

        #region Logging

        private static void LogRequestDetails(IRequestContext context) =>
            context.Log.Info("Sending request '{Request}'. Timeout = {Timeout}. Strategy = '{Strategy}'.",
                context.Request.ToString(false, false), context.Budget.Total.ToPrettyString(), context.Parameters.Strategy?.ToString());

        private static void LogSuccessfulResult(IRequestContext context, ClusterResult result) =>
            context.Log.Info(
                "Success. Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.",
                new
                {
                    ResponseCode = result.Response.Code,
                    ElapsedTime = context.Budget.Elapsed.ToPrettyString()
                });

        private static void LogFailedResult(IRequestContext context, ClusterResult result)
        {
            var message = "Failed with status '{Status}'. Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.";
            var properties = new
            {
                result.Status,
                ResponseCode = result.Response.Code,
            };

            if (result.Status == ClusterResultStatus.Canceled)
                context.Log.Warn(message, properties);
            else
                context.Log.Error(message, properties);
        }

        #endregion
    }
}