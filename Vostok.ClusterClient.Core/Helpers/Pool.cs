using System;
using System.Collections.Concurrent;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal class Pool<T>
        where T : class
    {
        private readonly Func<T> resourceFactory;
        private readonly ConcurrentQueue<T> resourceStorage;

        public Pool(Func<T> resourceFactory)
        {
            this.resourceFactory = resourceFactory;

            resourceStorage = new ConcurrentQueue<T>();
        }

        public PoolHandle<T> AcquireHandle() => new PoolHandle<T>(this, Acquire());

        public T Acquire()
        {
            if (!resourceStorage.TryDequeue(out var resource))
                resource = resourceFactory();

            return resource;
        }

        public void Release(T resource) => resourceStorage.Enqueue(resource);
    }
}