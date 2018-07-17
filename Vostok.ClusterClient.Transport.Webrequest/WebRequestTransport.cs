using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Commons.Collections;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.ClusterClient.Transport.Webrequest
{
    /// <summary>
    /// <para>Represents an <see cref="ITransport"/> implementation which uses <see cref="HttpWebRequest"/> to send requests to replicas.</para>
    /// <para>You can also use <see cref="IClusterClientConfigurationExtensions.SetupWebRequestTransport(Core.IClusterClientConfiguration)"/> extension to set up this transport in your configuration.</para>
    /// </summary>
    public class WebRequestTransport : ITransport
    {
        private const int PreferredReadSize = 16*1024;
        private const int LOHObjectSizeThreshold = 85*1000;

        private static readonly Pool<byte[]> ReadBuffersPool = new Pool<byte[]>(() => new byte[PreferredReadSize]);

        private readonly ILog log;
        private readonly ConnectTimeLimiter connectTimeLimiter;
        private readonly ThreadPoolMonitor threadPoolMonitor;

        public WebRequestTransport(WebRequestTransportSettings settings, ILog log)
        {
            Settings = settings;

            this.log = log ?? throw new ArgumentNullException(nameof(log));

            connectTimeLimiter = new ConnectTimeLimiter(settings, log);
            threadPoolMonitor = ThreadPoolMonitor.Instance;
        }

        public WebRequestTransport(ILog log)
            : this(new WebRequestTransportSettings(), log)
        {
        }

        public WebRequestTransportSettings Settings { get; }

        public TransportCapabilities Capabilities =>
            TransportCapabilities.RequestStreaming |
            TransportCapabilities.ResponseStreaming;

        public async Task<Response> SendAsync(Request request, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (timeout.TotalMilliseconds < 1)
            {
                LogRequestTimeout(request, timeout);
                return new Response(ResponseCode.RequestTimeout);
            }

            var state = new WebRequestState(timeout);

            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(state.TimeRemaining, timeoutCancellation.Token);
                var senderTask = SendInternalAsync(request, state, cancellationToken);
                var completedTask = await Task.WhenAny(timeoutTask, senderTask).ConfigureAwait(false);
                if (completedTask is Task<Response> taskWithResponse)
                {
                    timeoutCancellation.Cancel();
                    return taskWithResponse.GetAwaiter().GetResult();
                }

                // (iloktionov): Если выполнившееся задание не кастуется к Task<Response>, сработал таймаут.
                state.CancelRequest();
                LogRequestTimeout(request, timeout);

                if (Settings.FixThreadPoolProblems)
                    threadPoolMonitor.ReportAndFixIfNeeded(log);

                // (iloktionov): Попытаемся дождаться завершения задания по отправке запроса перед тем, как возвращать результат:
                var senderTaskContinuation = senderTask.ContinueWith(
                    t =>
                    {
                        if (t.IsCompleted)
                            t.GetAwaiter().GetResult().Dispose();
                    });

                var abortWaitingDelay = Task.Delay(Settings.RequestAbortTimeout);

                await Task.WhenAny(senderTaskContinuation, abortWaitingDelay).ConfigureAwait(false);

                if (!senderTask.IsCompleted)
                    LogFailedToWaitForRequestAbort();

                return ResponseFactory.BuildResponse(ResponseCode.RequestTimeout, state);
            }
        }

        private async Task<Response> SendInternalAsync(Request request, WebRequestState state, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(state.CancelRequest))
            {
                for (state.ConnectionAttempt = 1; state.ConnectionAttempt <= Settings.ConnectionAttempts; state.ConnectionAttempt++)
                {
                    using (state)
                    {
                        if (state.RequestCancelled)
                            return new Response(ResponseCode.Canceled);

                        state.Reset();
                        state.Request = WebRequestFactory.Create(request, state.TimeRemaining, Settings, log);

                        HttpActionStatus status;

                        // (iloktionov): Шаг 1 - отправить тело запроса, если оно имеется.
                        if (state.RequestCancelled)
                            return new Response(ResponseCode.Canceled);

                        if (request.HasBody)
                        {
                            status = await connectTimeLimiter.LimitConnectTime(SendRequestBodyAsync(request, state), request, state, Settings.ConnectionTimeout).ConfigureAwait(false);

                            if (status == HttpActionStatus.ConnectionFailure)
                                continue;

                            if (status != HttpActionStatus.Success)
                                return ResponseFactory.BuildFailureResponse(status, state);
                        }

                        // (iloktionov): Шаг 2 - получить ответ от сервера.
                        if (state.RequestCancelled)
                            return new Response(ResponseCode.Canceled);

                        status = request.HasBody
                            ? await GetResponseAsync(request, state).ConfigureAwait(false)
                            : await connectTimeLimiter.LimitConnectTime(GetResponseAsync(request, state), request, state, Settings.ConnectionTimeout).ConfigureAwait(false);

                        if (status == HttpActionStatus.ConnectionFailure)
                            continue;

                        if (status != HttpActionStatus.Success)
                            return ResponseFactory.BuildFailureResponse(status, state);

                        // (iloktionov): Шаг 3 - скачать тело ответа, если оно имеется.
                        if (!NeedToReadResponseBody(request, state))
                            return ResponseFactory.BuildSuccessResponse(state);

                        if (ResponseBodyIsTooLarge(state))
                        {
                            state.CancelRequestAttempt();
                            return ResponseFactory.BuildResponse(ResponseCode.InsufficientStorage, state);
                        }

                        if (state.RequestCancelled)
                            return new Response(ResponseCode.Canceled);

                        if (NeedToStreamResponseBody(state))
                        {
                            state.ReturnStreamDirectly = true;
                            state.PreventNextDispose();
                            return ResponseFactory.BuildSuccessResponse(state);
                        }

                        status = await ReadResponseBodyAsync(request, state).ConfigureAwait(false);
                        return status == HttpActionStatus.Success
                            ? ResponseFactory.BuildSuccessResponse(state)
                            : ResponseFactory.BuildFailureResponse(status, state);
                    }
                }

                return new Response(ResponseCode.ConnectFailure);
            }
        }

        private async Task<HttpActionStatus> SendRequestBodyAsync(Request request, WebRequestState state)
        {
            try
            {
                state.RequestStream = await state.Request.GetRequestStreamAsync().ConfigureAwait(false);
            }
            catch (WebException error)
            {
                return HandleWebException(request, state, error);
            }
            catch (Exception error)
            {
                LogUnknownException(error);
                return HttpActionStatus.UnknownFailure;
            }

            try
            {
                if (request.Content != null)
                {
                    await state.RequestStream.WriteAsync(request.Content.Buffer, request.Content.Offset, request.Content.Length).ConfigureAwait(false);
                }
                else if (request.StreamContent != null)
                {
                    var bodyStream = request.StreamContent.Stream;
                    var bytesToSend = request.StreamContent.Length ?? long.MaxValue;
                    var bytesSent = 0L;

                    using (var bufferHandle = ReadBuffersPool.AcquireHandle())
                    {
                        var buffer = bufferHandle.Resource;

                        while (bytesSent < bytesToSend)
                        {
                            var bytesToRead = (int)Math.Min(buffer.Length, bytesToSend - bytesSent);

                            int bytesRead;

                            try
                            {
                                bytesRead = await bodyStream.ReadAsync(buffer, 0, bytesToRead).ConfigureAwait(false);
                            }
                            catch (StreamAlreadyUsedException)
                            {
                                throw;
                            }
                            catch (Exception error)
                            {
                                if (IsCancellationException(error))
                                    return HttpActionStatus.RequestCanceled;

                                LogUserStreamFailure(error);

                                return HttpActionStatus.UserStreamFailure;
                            }

                            if (bytesRead == 0)
                                break;

                            await state.RequestStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                            bytesSent += bytesRead;
                        }
                    }
                }

                state.CloseRequestStream();
            }
            catch (StreamAlreadyUsedException)
            {
                throw;
            }
            catch (Exception error)
            {
                if (IsCancellationException(error))
                    return HttpActionStatus.RequestCanceled;

                LogSendBodyFailure(request, error);
                return HttpActionStatus.SendFailure;
            }

            return HttpActionStatus.Success;
        }

        private async Task<HttpActionStatus> GetResponseAsync(Request request, WebRequestState state)
        {
            try
            {
                state.Response = (HttpWebResponse)await state.Request.GetResponseAsync().ConfigureAwait(false);
                state.ResponseStream = state.Response.GetResponseStream();
                return HttpActionStatus.Success;
            }
            catch (WebException error)
            {
                var status = HandleWebException(request, state, error);
                // (iloktionov): HttpWebRequest реагирует на коды ответа вроде 404 или 500 исключением со статусом ProtocolError.
                if (status == HttpActionStatus.ProtocolError)
                {
                    state.Response = (HttpWebResponse)error.Response;
                    state.ResponseStream = state.Response.GetResponseStream();
                    return HttpActionStatus.Success;
                }

                return status;
            }
            catch (Exception error)
            {
                LogUnknownException(error);
                return HttpActionStatus.UnknownFailure;
            }
        }

        private static bool NeedToReadResponseBody(Request request, WebRequestState state)
        {
            if (request.Method == RequestMethods.Head)
                return false;

            // (iloktionov): ContentLength может быть равен -1, если сервер не укажет заголовок, но вернет контент. Это умолчание на уровне HttpWebRequest.
            return state.Response.ContentLength != 0;
        }

        private bool NeedToStreamResponseBody(WebRequestState state)
        {
            try
            {
                var contentLength = null as long?;

                if (state.Response.ContentLength >= 0)
                    contentLength = state.Response.ContentLength;

                return Settings.UseResponseStreaming(contentLength);
            }
            catch (Exception error)
            {
                log.Error(error);
                return false;
            }
        }

        private bool ResponseBodyIsTooLarge(WebRequestState state)
        {
            var size = Math.Max(state.Response.ContentLength, state.BodyStream?.Length ?? 0L);
            var limit = Settings.MaxResponseBodySize?.Bytes ?? long.MaxValue;

            if (size > limit)
                LogResponseBodyTooLarge(size, limit);

            return size > limit;
        }

        private async Task<HttpActionStatus> ReadResponseBodyAsync(Request request, WebRequestState state)
        {
            try
            {
                var contentLength = (int)state.Response.ContentLength;
                if (contentLength > 0)
                {
                    state.BodyBuffer = Settings.BufferFactory(contentLength);

                    var totalBytesRead = 0;

                    // (iloktionov): Если буфер размером contentLength не попадет в LOH, можно передать его напрямую для работы с сокетом.
                    // В противном случае лучше использовать небольшой промежуточный буфер из пула, т.к. ссылка на переданный сохранится надолго из-за Keep-Alive.
                    if (contentLength < LOHObjectSizeThreshold)
                        while (totalBytesRead < contentLength)
                        {
                            var bytesToRead = Math.Min(contentLength - totalBytesRead, PreferredReadSize);
                            var bytesRead = await state.ResponseStream.ReadAsync(state.BodyBuffer, totalBytesRead, bytesToRead).ConfigureAwait(false);
                            if (bytesRead == 0)
                                break;

                            totalBytesRead += bytesRead;
                        }
                    else
                        using (var bufferHandle = ReadBuffersPool.AcquireHandle())
                        {
                            var buffer = bufferHandle.Resource;

                            while (totalBytesRead < contentLength)
                            {
                                var bytesToRead = Math.Min(contentLength - totalBytesRead, buffer.Length);
                                var bytesRead = await state.ResponseStream.ReadAsync(buffer, 0, bytesToRead).ConfigureAwait(false);
                                if (bytesRead == 0)
                                    break;

                                Buffer.BlockCopy(buffer, 0, state.BodyBuffer, totalBytesRead, bytesRead);

                                totalBytesRead += bytesRead;
                            }
                        }

                    if (totalBytesRead < contentLength)
                        throw new EndOfStreamException($"Response stream ended prematurely. Read only {totalBytesRead} byte(s), but Content-Length specified {contentLength}.");

                    state.BodyBufferLength = totalBytesRead;
                }
                else
                {
                    state.BodyStream = new MemoryStream();

                    using (var bufferHandle = ReadBuffersPool.AcquireHandle())
                    {
                        var buffer = bufferHandle.Resource;

                        while (true)
                        {
                            var bytesRead = await state.ResponseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            if (bytesRead == 0)
                                break;

                            state.BodyStream.Write(buffer, 0, bytesRead);

                            if (ResponseBodyIsTooLarge(state))
                            {
                                state.CancelRequestAttempt();
                                state.BodyStream = null;
                                return HttpActionStatus.InsufficientStorage;
                            }
                        }
                    }
                }

                return HttpActionStatus.Success;
            }
            catch (Exception error)
            {
                if (IsCancellationException(error))
                    return HttpActionStatus.RequestCanceled;

                LogReceiveBodyFailure(request, error);
                return HttpActionStatus.ReceiveFailure;
            }
        }

        private HttpActionStatus HandleWebException(Request request, WebRequestState state, WebException error)
        {
            switch (error.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.ConnectionClosed:
                case WebExceptionStatus.PipelineFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                case WebExceptionStatus.SecureChannelFailure:
                    LogConnectionFailure(request, error, state.ConnectionAttempt);
                    return HttpActionStatus.ConnectionFailure;
                case WebExceptionStatus.SendFailure:
                    LogWebException(error);
                    return HttpActionStatus.SendFailure;
                case WebExceptionStatus.ReceiveFailure:
                    LogWebException(error);
                    return HttpActionStatus.ReceiveFailure;
                case WebExceptionStatus.RequestCanceled: return HttpActionStatus.RequestCanceled;
                case WebExceptionStatus.Timeout: return HttpActionStatus.Timeout;
                case WebExceptionStatus.ProtocolError: return HttpActionStatus.ProtocolError;
                default:
                    LogWebException(error);
                    return HttpActionStatus.UnknownFailure;
            }
        }

        private static bool IsCancellationException(Exception error) =>
            error is OperationCanceledException || (error as WebException)?.Status == WebExceptionStatus.RequestCanceled;

        #region Logging

        private void LogRequestTimeout(Request request, TimeSpan timeout) =>
            log.Error($"Request timed out. Target = {request.Url.Authority}. Timeout = {timeout.ToPrettyString()}.");

        private void LogConnectionFailure(Request request, WebException error, int attempt)
        {
            var message = $"Connection failure. Target = {request.Url.Authority}. Attempt = {attempt}/{Settings.ConnectionAttempts}. Status = {error.Status}.";
            var exception = error.InnerException ?? error;

            if (attempt == Settings.ConnectionAttempts)
                log.Error(exception, message);
            else
                log.Warn(exception, message);
        }

        private void LogWebException(WebException error) =>
            log.Error(error.InnerException ?? error, $"Error in sending request. Status = {error.Status}.");

        private void LogUnknownException(Exception error) =>
            log.Error(error, "Unknown error in sending request.");

        private void LogSendBodyFailure(Request request, Exception error) =>
            log.Error(error, "Error in sending request body to " + request.Url.Authority);

        private void LogUserStreamFailure(Exception error) =>
            log.Error(error, "Failure in reading input stream while sending request body.");

        private void LogReceiveBodyFailure(Request request, Exception error) =>
            log.Error(error, "Error in receiving request body from " + request.Url.Authority);

        private void LogFailedToWaitForRequestAbort() =>
            log.Warn($"Timed out request was aborted but did not complete in {Settings.RequestAbortTimeout.ToPrettyString()}.");

        private void LogResponseBodyTooLarge(long size, long limit) =>
            log.Error($"Response body size {size} is larger than configured limit of {limit} bytes.");

        #endregion
    }
}