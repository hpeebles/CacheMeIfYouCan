using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;

namespace CacheMeIfYouCan.Internal
{
    public class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>
    {
        private readonly Func<int, TCollection> _factory;
        private readonly DefaultObjectPool<TCollection>[] _buckets = new DefaultObjectPool<TCollection>[BucketCount];
        private const int BucketCount = 17;
        
        public CollectionPool(Func<int, TCollection> factory)
        {
            _factory = factory;
            Initialize();
        }

        public TCollection Rent(int minCapacity)
        {
            var bucketIndex = GetBucketIndex(minCapacity);

            return bucketIndex < _buckets.Length
                ? _buckets[bucketIndex].Get()
                : _factory(minCapacity);
        }

        public void Return(TCollection collection)
        {
            var bucketIndex = GetBucketIndex(collection.Count);

            if (bucketIndex >= _buckets.Length)
                return;
            
            collection.Clear();
            _buckets[bucketIndex].Return(collection);
        }

        private static int GetBucketIndex(int minCapacity)
        {
            var bits = ((uint)minCapacity - 1) >> 4;
            return 32 - BitOperations.LeadingZeroCount(bits);
        }
        
        private void Initialize()
        {
            const int minCapacity = 16;
            for (var i = 0; i < BucketCount; i++)
            {
                var capacity = minCapacity << i;
                var policy = new PooledCollectionPolicy(() => _factory(capacity));
                _buckets[i] = new DefaultObjectPool<TCollection>(policy);
            }
        }

        private sealed class PooledCollectionPolicy : PooledObjectPolicy<TCollection>
        {
            private readonly Func<TCollection> _factory;

            public PooledCollectionPolicy(Func<TCollection> factory)
            {
                _factory = factory;
            }

            public override TCollection Create() => _factory();
            public override bool Return(TCollection collection) => true;
        }
    }
}