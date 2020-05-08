using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.ObjectPool;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class HighCapacityObjectPool<T> : ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly ChannelReader<T> _reader;
        private readonly ChannelWriter<T> _writer;
        private T _firstItem;

        public HighCapacityObjectPool(Func<T> factory, int capacity)
        {
            if (capacity <= 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            
            _factory = factory;
            
            var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity - 1));

            _reader = channel.Reader;
            _writer = channel.Writer;
        }
        
        public override T Get()
        {
            var firstItem = _firstItem;
            if (!(firstItem is null) && Interlocked.CompareExchange(ref _firstItem, null, firstItem) == firstItem)
                return firstItem;
            
            return _reader.TryRead(out var item)
                ? item
                : _factory();
        }

        public override void Return(T item)
        {
            if (_firstItem is null && Interlocked.CompareExchange(ref _firstItem, item, null) == null)
                return;
            
            _writer.TryWrite(item);
        }

        // Only use this for tests!
        public List<T> PeekAll()
        {
            var list = new List<T>();

            var firstItem = _firstItem;
            if (!(firstItem is null))
                list.Add(firstItem);
            
            while (_reader.TryRead(out var next))
                list.Add(next);

            foreach (var item in list.Skip(1))
                _writer.TryWrite(item);

            return list;
        }
    }
}