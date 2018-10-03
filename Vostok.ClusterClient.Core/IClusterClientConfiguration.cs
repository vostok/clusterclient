using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Misc;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Ordering;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Retry;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.ClusterClient.Core.Transport;
using Vostok.ClusterClient.Core.Ordering.Weighed;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core
{
    /// <summary>
    /// <para>Represents a configuration of <see cref="ClusterClient"/> instance which must be filled during client construction.</para>
    /// <para>The only required parameters are <see cref="Transport"/> and <see cref="ClusterProvider"/>.</para>
    /// </summary>
    [PublicAPI]
    public interface IClusterClientConfiguration
    {
        /// <summary>
        /// Returns an <see cref="ILog"/> instance which can be used to construct other parts of configuration.
        /// </summary>
        [NotNull]
        ILog Log { get; }

        /// <summary>
        /// <para>A transport (HTTP client) implementation used to send requests. See <see cref="ITransport"/> for more details.</para>
        /// <para>This parameter is REQUIRED.</para>
        /// </summary>
        ITransport Transport { get; set; }

        /// <summary>
        /// <para>An implementation of cluster provider. See <see cref="IClusterProvider.GetCluster"/> for more details.</para>
        /// <para>This parameter is REQUIRED.</para>
        /// </summary>
        IClusterProvider ClusterProvider { get; set; }

        /// <summary>
        /// <para>Gets or sets replica addresses transform. See <see cref="IReplicaTransform"/> for more details.</para>
        /// <para>This parameter is optional.</para>
        /// </summary>
        IReplicaTransform ReplicaTransform { get; set; }

        /// <summary>
        /// <para>Gets or sets replica ordering implementation. See <see cref="IReplicaOrdering.Order"/> and <see cref="IReplicaOrdering.Learn"/> for more details.</para>
        /// <para>The recommended implementation is <see cref="WeighedReplicaOrdering"/>. Use <see cref="ClusterClientConfigurationExtensions.SetupWeighedReplicaOrdering"/> extension to build it.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.ReplicaOrdering"/>).</para>
        /// </summary>
        IReplicaOrdering ReplicaOrdering { get; set; }

        /// <summary>
        /// <para>Gets or sets the replica storage scope. See <see cref="ReplicaStorageScope"/> and <see cref="IReplicaStorageProvider"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Ordering.Storage.ReplicaStorageScope"/>).</para>
        /// </summary>
        ReplicaStorageScope ReplicaStorageScope { get; set; }

        /// <summary>
        /// <para>A list of request transforms. See <see cref="IRequestTransform"/> for more details.</para>
        /// <para>Use <see cref="ClusterClientConfigurationExtensions.AddRequestTransform(IClusterClientConfiguration, IRequestTransform)"/> to add transforms to this list.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        List<IRequestTransform> RequestTransforms { get; set; }

        /// <summary>
        /// <para>A list of response transforms. See <see cref="IResponseTransform"/> for more details.</para>
        /// <para>Use <see cref="ClusterClientConfigurationExtensions.AddResponseTransform(IClusterClientConfiguration, IResponseTransform)"/> to add transforms to this list.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        List<IResponseTransform> ResponseTransforms { get; set; }

        /// <summary>
        /// <para>A list of response criteria. See <see cref="IResponseCriterion"/> and <see cref="ResponseVerdict"/> for more details.</para>
        /// <para>Use <see cref="ClusterClientConfigurationExtensions.SetupResponseCriteria"/> to initialize this list.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.ResponseCriteria"/>).</para>
        /// </summary>
        List<IResponseCriterion> ResponseCriteria { get; set; }

        /// <summary>
        /// <para>A collection of additional user-defined request modules. These modules are inserted into native execution pipeline.</para>
        /// <para>User-defined modules inserted into pipeline into specified <see cref="RequestPipelinePoint"/>.</para>
        /// <para>See <see cref="IRequestModule"/> interface for more details about request modules.</para>
        /// <para>Final execution pipeline looks like this:</para>
        /// <list type="number">
        /// <item><description>Underlying response streams closing.</description></item>
        /// <item><description>Exception logging and handling.</description></item>
        /// <item><description>Request transformation (application of <see cref="IRequestTransform"/> chain).</description></item>
        /// <item><description>Request priority application (adding a priority header to request).</description></item>
        /// <item><description>Client application identity (adding a client application header to request).</description></item>
        /// <item><description><see cref="RequestPipelinePoint.AfterPrepareRequest"/>. User-defined <see cref="IRequestModule"/> implementations(by default inserted here).</description></item>
        /// <item><description>Request/result logging.</description></item>
        /// <item><description>Response transformation (application of <see cref="IResponseTransform"/> chain).</description></item>
        /// <item><description>Exception logging and handling.</description></item>
        /// <item><description>Request validation.</description></item>
        /// <item><description><see cref="RequestPipelinePoint.AfterRequestValidation"/>.</description></item>
        /// <item><description>Timeout validation.</description></item>
        /// <item><description>try loop (application of <see cref="IRetryPolicy"/> and <see cref="IRetryStrategy"/>).</description></item>
        /// <item><description><see cref="RequestPipelinePoint.BeforeSend"/>.</description></item>
        /// <item><description>Sending of requests with absolute urls (directly using <see cref="ITransport"/>).</description></item>
        /// <item><description><see cref="RequestPipelinePoint.BeforeExecution"/>.</description></item>
        /// <item><description>Request execution (<see cref="IClusterProvider"/> --> <see cref="IReplicaOrdering"/> --> <see cref="IRequestStrategy"/>)</description></item>
        /// </list>
        /// <para>Use <see cref="ClusterClientConfigurationExtensions.AddRequestModule"/> to add transforms to this collection.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        Dictionary<RequestPipelinePoint, List<IRequestModule>> Modules { get; set; }

        /// <summary>
        /// <para>Gets or sets retry policy. See <see cref="IRetryPolicy"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RetryPolicy"/>).</para>
        /// </summary>
        IRetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// <para>Gets or sets retry strategy. See <see cref="IRetryStrategy"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RetryStrategy"/>).</para>
        /// </summary>
        IRetryStrategy RetryStrategy { get; set; }

        /// <summary>
        /// <para>Gets or sets the response selector. See <see cref="IResponseSelector.Select"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.ResponseSelector"/>).</para>
        /// </summary>
        IResponseSelector ResponseSelector { get; set; }

        /// <summary>
        /// <para>Gets or sets a default request strategy used for <see cref="ClusterClient"/> method overloads without strategy parameter.</para>
        /// <para>See <see cref="IRequestStrategy.SendAsync"/> for more details about what a request strategy is.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RequestStrategy"/>).</para>
        /// </summary>
        IRequestStrategy DefaultRequestStrategy { get; set; }

        /// <summary>
        /// <para>Gets or sets a default request timeout used for <see cref="ClusterClient"/> method overloads without timeout parameter.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Timeout"/>).</para>
        /// </summary>
        TimeSpan DefaultTimeout { get; set; }

        /// <summary>
        /// <para>Gets or sets a default request priority used for <see cref="ClusterClient"/> method overloads without priority parameter.</para>
        /// <para>This parameter is optional and has a <c>null</c> default value.</para>
        /// </summary>
        RequestPriority? DefaultPriority { get; set; }

        /// <summary>
        /// <para>Gets or sets a limit on how many replicas a single request may use. Such a limit is useful to contain uncontrollable retry explosions.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.MaxReplicasUsedPerRequest"/>).</para>
        /// </summary>
        int MaxReplicasUsedPerRequest { get; set; }

        /// <summary>
        /// <para>Gets or sets the options for request/response logging.</para>
        /// <para>This parameter is optional and has a <c>null</c> default value which implies default options will be used.</para>
        /// </summary>
        LoggingOptions Logging { get; set; }

        /// <summary>
        /// <para>Gets or sets the name of client application which use ClusterClient. </para>
        /// <para>This parameter is optional and by default set to <see cref="Assembly.GetEntryAssembly"/> name.</para>
        /// </summary>
        string ClientApplicationName { get; set; }

        /// <summary>
        /// <para>Gets or sets the name of service this <see cref="ClusterClient"/> will talk to.</para>
        /// <para>This parameter is optional and has no default value.</para>
        /// </summary>
        string ServiceName { get; set; }

        /// <summary>
        /// <para>Gets or sets the environment.</para>
        /// <para>This parameter is optional and has no default value.</para>
        /// </summary>
        string Environment { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to remove duplicate path segments from beginning of request url.</para>
        /// <para>A prefix from multiple segments is called a duplicate when it's also a suffix of replica url.</para>
        /// <para>Example replica url: http://api.kontur.ru/drive/v1</para>
        /// <para>Example request url: v1/contents/foo/bar</para>
        /// <para>Example duplicate path segment is "v1".</para>
        /// <para>Only works for requests with relative urls.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.DeduplicateRequestUrl"/>).</para>
        /// </summary>
        bool DeduplicateRequestUrl { get; set; }
    }
}
