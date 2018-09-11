namespace Vostok.ClusterClient.Core.Misc
{
    public class LoggingOptions
    {
        /// <summary>
        /// <para>Gets or sets whether to log request details before execution.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.LogRequestDetails"/>).</para>
        /// </summary>
        public bool LogRequestDetails { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to log result details after execution.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.LogResultDetails"/>).</para>
        /// </summary>
        public bool LogResultDetails { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to log requests to each replica.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.LogReplicaRequests"/>).</para>
        /// </summary>
        public bool LogReplicaRequests { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to log results from each replica.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.LogReplicaResults"/>).</para>
        /// </summary>
        public bool LogReplicaResults { get; set; }

        /// <summary>
        /// <para>Gets or sets whether to add sequential log prefix to each request's messages.</para>
        /// <para>This parameter is optional and has a default value (see <see cref="ClusterClientDefaults.LogPrefixEnabled"/>).</para>
        /// </summary>
        public bool LogPrefixEnabled { get; set; }
    }
}