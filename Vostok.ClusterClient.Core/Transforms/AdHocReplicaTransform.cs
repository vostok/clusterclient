using System;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// Represents a replica transform which uses external delegate to modify urls.
    /// </summary>
    public class AdHocReplicaTransform : IReplicaTransform
    {
        private readonly Func<Uri, Uri> transform;

        /// <summary>
        /// Initializes a new instance of <see cref="AdHocReplicaTransform"/> class.
        /// </summary>
        /// <param name="transform">An external delegate which will be used to modify urls.</param>
        public AdHocReplicaTransform(Func<Uri, Uri> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Uri Transform(Uri replica) => transform(replica);
    }
}