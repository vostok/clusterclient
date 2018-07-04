using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal struct PoolHandle<T> : IDisposable
        where T : class
    {
        private readonly Pool<T> pool;

        public PoolHandle(Pool<T> pool, T resource)
        {
            this.pool = pool;
            Resource = resource;
        }

        public T Resource { get; }

        public void Dispose() => pool.Release(Resource);

        public static implicit operator T(PoolHandle<T> handle) => handle.Resource;
    }
}