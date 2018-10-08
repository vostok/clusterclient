using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Transforms
{
    /// <summary>
    /// A set of <see cref="IReplicaTransform"/> extensions.
    /// </summary>
    public static class ReplicaTransformExtensions
    {
        /// <summary>
        /// Applies <paramref name="transform"/> for each <see cref="Uri"/> in <paramref name="replicas"/>.
        /// </summary>
        /// <param name="transform">A <see cref="IReplicaTransform"/> which will be applied</param>.
        /// <param name="replicas">A list of replica <see cref="Uri"/> for transformation</param>.
        /// <returns></returns>
        public static IList<Uri> Transform(this IReplicaTransform transform, IList<Uri> replicas)
        {
            var transformed = new Uri[replicas.Count];

            for (var i = 0; i < replicas.Count; i++)
            {
                transformed[i] = transform.Transform(replicas[i]);
            }

            return transformed;
        }
    }
}