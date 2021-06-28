using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core
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
        /// <para>A list of replica filters. See <see cref="IReplicasFilter"/> for more details.</para>
        /// <para>Use <see cref="IClusterClientConfigurationExtensions.AddReplicasFilter"/> to add filters to this list.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        List<IReplicasFilter> ReplicasFilters { get; set; }
        
        /// <summary>
        /// <para>Gets or sets replica addresses transform. See <see cref="IReplicaTransform"/> for more details.</para>
        /// <para>This parameter is optional.</para>
        /// </summary>
        IReplicaTransform ReplicaTransform { get; set; }

        /// <summary>
        /// <para>Gets or sets replica ordering implementation. See <see cref="IReplicaOrdering.Order"/> and <see cref="IReplicaOrdering.Learn"/> for more details.</para>
        /// <para>The recommended implementation is <see cref="WeighedReplicaOrdering"/>. Use <see cref="IClusterClientConfigurationExtensions.SetupWeighedReplicaOrdering"/> extension to build it.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.ReplicaOrdering"/>).</para>
        /// </summary>
        IReplicaOrdering ReplicaOrdering { get; set; }

        /// <summary>
        /// <para>Gets or sets the replica storage scope. See <see cref="ReplicaStorageScope"/> and <see cref="IReplicaStorageProvider"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Ordering.Storage.ReplicaStorageScope"/>).</para>
        /// </summary>
        ReplicaStorageScope ReplicaStorageScope { get; set; }

        /// <summary>
        /// <para>A list of request transforms. See <see cref="IRequestTransform"/> and <see cref="IAsyncRequestTransform"/> for more details.</para>
        /// <para>Use <see cref="IClusterClientConfigurationExtensions.AddRequestTransform(IClusterClientConfiguration, IRequestTransform)"/> to add transforms to this list.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        List<IRequestTransformMetadata> RequestTransforms { get; set; }

        /// <summary>
        /// <para>A list of response transforms. See <see cref="IResponseTransform"/> for more details.</para>
        /// <para>Use <see cref="IClusterClientConfigurationExtensions.AddResponseTransform(IClusterClientConfiguration, IResponseTransform)"/> to add transforms to this list.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        List<IResponseTransform> ResponseTransforms { get; set; }

        /// <summary>
        /// <para>A list of response criteria. See <see cref="IResponseCriterion"/> and <see cref="ResponseVerdict"/> for more details.</para>
        /// <para>Use <see cref="IClusterClientConfigurationExtensions.SetupResponseCriteria"/> to initialize this list.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.ResponseCriteria"/>).</para>
        /// </summary>
        List<IResponseCriterion> ResponseCriteria { get; set; }

        /// <summary>
        /// <para>A collection of user-defined request modules. These modules are inserted into native execution pipeline.</para>
        /// <para>User-defined modules inserted into pipeline near module of specified <see cref="Type"/>.</para>
        /// <para>See <see cref="IRequestModule"/> interface for more details about request modules.</para>
        /// <para>Final execution pipeline looks like this:</para>
        /// <list type="number">
        /// <item><description><see cref="RequestModule.LeakPrevention"/>: underlying response streams cleanup.</description></item>
        /// <item><description><see cref="RequestModule.GlobalErrorCatching"/>: exception logging and handling.</description></item>
        /// <item><description><see cref="RequestModule.RequestTransformation"/>: request transformation (application of <see cref="IRequestTransform"/> chain).</description></item>
        /// <item><description><see cref="RequestModule.AuxiliaryHeaders"/>: request priority application (adding a priority header to request).</description></item>
        /// <item><description>User-defined modules.</description></item>
        /// <item><description><see cref="RequestModule.ThreadPoolTuning"/>: <see cref="System.Threading.ThreadPool"/> limits tuning.</description></item>
        /// <item><description><see cref="RequestModule.Logging"/>: request/result logging.</description></item>
        /// <item><description><see cref="RequestModule.ResponseTransformation"/>: response transformation (application of <see cref="IResponseTransform"/> chain).</description></item>
        /// <item><description><see cref="RequestModule.ErrorCatching"/>: exception logging and handling.</description></item>
        /// <item><description><see cref="RequestModule.RequestValidation"/>: request validation.</description></item>
        /// <item><description><see cref="RequestModule.TimeoutValidation"/>: timeout validation.</description></item>
        /// <item><description><see cref="RequestModule.RequestRetry"/>: try loop (application of <see cref="IRetryPolicy"/> and <see cref="IRetryStrategy"/>).</description></item>
        /// <item><description><see cref="RequestModule.AbsoluteUrlSender"/>: sending of requests with absolute urls (directly using <see cref="ITransport"/>).</description></item>
        /// <item><description><see cref="RequestModule.RequestExecution"/>: request execution (<see cref="IClusterProvider"/> --> <see cref="IReplicaOrdering"/> --> <see cref="IRequestStrategy"/>)</description></item>
        /// </list>
        /// <para>Use <c>AddRequestModule</c> method from <see cref="IClusterClientConfigurationExtensions"/> to add modules to this collection.</para>
        /// <para>This parameter is optional and has an empty default value.</para>
        /// </summary>
        IDictionary<Type, RelatedModules> Modules { get; set; }

        /// <summary>
        /// A set of built-in modules to be removed from standard pipeline.
        /// </summary>
        ISet<Type> ModulesToRemove { get; set; }

        /// <summary>
        /// <para>Gets or sets retry policy. See <see cref="IRetryPolicy"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RetryPolicy"/>).</para>
        /// </summary>
        IRetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// <para>This module type is deprecated. Use more general <see cref="IClusterClientConfiguration.RetryStrategyEx"/>.</para>
        /// <para>Gets or sets retry strategy. See <see cref="IRetryStrategy"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RetryStrategyEx"/>).</para>
        /// <para>All <see cref="IRetryStrategy"/> will be converted to a <see cref="IRetryStrategyEx"/> internally.</para>
        /// </summary>
        IRetryStrategy RetryStrategy { get; set; }

        /// <summary>
        /// <para>Gets or sets retry strategy. See <see cref="IRetryStrategyEx"/> for more details.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="Core.ClusterClientDefaults.RetryStrategyEx"/>).</para>
        /// </summary>
        IRetryStrategyEx RetryStrategyEx { get; set; }

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
        /// <para>Gets or sets the timeout to establish new TCP connection to replica in cluster.</para>
        /// <para>This parameter is optional and has default value <see cref="ClusterClientDefaults.ConnectionTimeout"/></para>
        /// <para>If set to <c>null</c>, no connection timeout will be used.</para>
        /// </summary>
        TimeSpan? DefaultConnectionTimeout { get; set; }

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
        /// <para>This parameter is optional. By default it is set to <see cref="Assembly.GetEntryAssembly"/> name.</para>
        /// </summary>
        string ClientApplicationName { get; set; }

        /// <summary>
        /// <para>Gets or sets the name of service this <see cref="ClusterClient"/> will talk to.</para>
        /// <para>This parameter is optional and has no default value.</para>
        /// </summary>
        string TargetServiceName { get; set; }

        /// <summary>
        /// <para>Gets or sets the target service environment.</para>
        /// <para>This parameter is optional and has no default value.</para>
        /// </summary>
        string TargetEnvironment { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to remove duplicate path segments from beginning of request url.</para>
        /// <para>A prefix from multiple segments is called a duplicate when it's also a suffix of replica url.</para>
        /// <para>Example replica url: http://api.kontur.ru/drive/v1</para>
        /// <para>Example request url: v1/contents/foo/bar</para>
        /// <para>Example duplicate path segment is "v1".</para>
        /// <para>Only works for requests with relative urls.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.DeduplicateRequestUrl"/>).</para>
        /// </summary>
        bool DeduplicateRequestUrl { get; set; }

        /// <summary>
        /// <para>Gets or sets a number of TCP connection establish attempts.</para>
        /// <para>This parameter is optional and by default set to <see cref="ClusterClientDefaults.ConnectionAttempts"/></para>
        /// </summary>
        int ConnectionAttempts { get; set; }
    }
}