using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Transforms
{
    /// <summary>
    /// Represents a replica transform which uses external delegate to modify urls.
    /// </summary>
    [PublicAPI]
    public class AdHocReplicaTransform : IReplicaTransform
    {
        private readonly Func<Uri, Uri> transform;

        /// <param name="transform">An external delegate which will be used to modify urls.</param>
        public AdHocReplicaTransform(Func<Uri, Uri> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Uri Transform(Uri replica) => transform(replica);
    }
}