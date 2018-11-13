namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    internal static class ReplicaStorageProviderFactory
    {
        private static readonly PerProcessReplicaStorageProvider SharedProvider = new PerProcessReplicaStorageProvider();

        public static IReplicaStorageProvider Create(ReplicaStorageScope scope) =>
            scope == ReplicaStorageScope.Process ? (IReplicaStorageProvider) SharedProvider : new PerInstanceReplicaStorageProvider();
    }
}