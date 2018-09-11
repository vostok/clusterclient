namespace Vostok.ClusterClient.Core.Model
{
    internal class ClusterResultFactory
    {
        internal static ClusterResult TimeExpired(Request request)
        {   
            return new ClusterResult(ClusterResultStatus.TimeExpired, new ReplicaResult[] {}, null, request);
        }

        internal static ClusterResult ReplicasNotFound(Request request)
        {
            return new ClusterResult(ClusterResultStatus.ReplicasNotFound, new ReplicaResult[] {}, null, request);
        }

        internal static ClusterResult IncorrectArguments(Request request)
        {
            return new ClusterResult(ClusterResultStatus.IncorrectArguments, new ReplicaResult[] {}, null, request);
        }

        internal static ClusterResult UnexpectedException(Request request)
        {
            return new ClusterResult(ClusterResultStatus.UnexpectedException, new ReplicaResult[] {}, null, request);
        }

        internal static ClusterResult Canceled(Request request)
        {
            return new ClusterResult(ClusterResultStatus.Canceled, new ReplicaResult[] {}, null, request);
        }

        internal static ClusterResult Throttled(Request request)
        {
            return new ClusterResult(ClusterResultStatus.Throttled, new ReplicaResult[] {}, null, request);
        }
 
    }
}