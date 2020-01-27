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
        private readonly string targetService;

        public LoggingModule(bool logRequests, bool logResults, string targetService)
        {
            this.logRequests = logRequests;
            this.logResults = logResults;
            this.targetService = targetService;
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

        private void LogRequestDetails(IRequestContext context) =>
            context.Log.Info("Sending request '{Request}' to '{TargetService}'. Timeout = {Timeout}. Strategy = '{Strategy}'.",
                new
                {
                    Request = context.Request.ToString(false, false),
                    TargetService = targetService ?? "somewhere",
                    Timeout = context.Budget.Total.ToPrettyString(),
                    TimeoutMs = context.Budget.Total.TotalMilliseconds,
                    Strategy = context.Parameters.Strategy?.ToString()
                });

        private void LogSuccessfulResult(IRequestContext context, ClusterResult result) =>
            context.Log.Info(
                "Success. Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.",
                new
                {
                    ResponseCode = result.Response.Code,
                    ElapsedTime = context.Budget.Elapsed.ToPrettyString(),
                    ElapsedTimeMs = context.Budget.Elapsed.TotalMilliseconds
                });

        private void LogFailedResult(IRequestContext context, ClusterResult result)
        {
            var message = "Request '{Request}' to '{TargetService}' has failed with status '{Status}'. Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.";
            var properties = new
            {
                Request = context.Request.ToString(false, false),
                TargetService = targetService ?? "somewhere",
                result.Status,
                ResponseCode = result.Response.Code,
                ElapsedTime = context.Budget.Elapsed.ToPrettyString(),
                ElapsedTimeMs = context.Budget.Elapsed.TotalMilliseconds
            };

            if (result.Status == ClusterResultStatus.Canceled)
                context.Log.Warn(message, properties);
            else
                context.Log.Error(message, properties);
        }

        #endregion
    }
}