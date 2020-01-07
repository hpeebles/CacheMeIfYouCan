using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class ObjectPool<T>
    {
        private readonly Func<T> _factory;
        private readonly ChannelReader<T> _reader;
        private readonly ChannelWriter<T> _writer;

        public ObjectPool(Func<T> factory, int capacity)
        {
            _factory = factory;
            
            var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity));

            _reader = channel.Reader;
            _writer = channel.Writer;
        }
        
        public T Rent()
        {
            return _reader.TryRead(out var item)
                ? item
                : _factory();
        }

        public void Return(T item)
        {
            _writer.TryWrite(item);
        }

        // Only use this for tests!
        public List<T> PeekAll()
        {
            var list = new List<T>();
            while (_reader.TryRead(out var next))
                list.Add(next);

            foreach (var item in list)
                _writer.TryWrite(item);

            return list;
        }
    }
}