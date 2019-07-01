using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class TestImpl : ITest
    {
        private readonly TimeSpan? _delay;

        public TestImpl(TimeSpan? delay = null)
        {
            _delay = delay;
        }

        public async Task<string> StringToString(string key)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);
            
            return key;
        }

        public async Task<string> IntToString(int key)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return key.ToString();
        }

        public async Task<int> LongToInt(long key)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            
            return (int) key * 2;
        }

        public async Task<IDictionary<string, string>> MultiEcho(IEnumerable<string> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return keys.ToDictionary(k => k);
        }
        
        public async Task<IDictionary<string, string>> MultiEchoList(IList<string> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return keys.ToDictionary(k => k);
        }
        
        public async Task<IDictionary<string, string>> MultiEchoSet(ISet<string> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return keys.ToDictionary(k => k);
        }

        public string StringToStringSync(string key)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).GetAwaiter().GetResult();

            return key;
        }

        public IDictionary<string, string> MultiStringToStringSync(ICollection<string> keys)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).GetAwaiter().GetResult();

            return keys.ToDictionary(k => k);
        }
        
        public async Task<ConcurrentDictionary<string, string>> MultiEchoToConcurrent(IEnumerable<string> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return new ConcurrentDictionary<string, string>(keys.ToDictionary(k => k));
        }

        public async Task<string> MultiParamEcho(string key1, int key2)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return $"{key1}_{key2}";
        }
        
        public string MultiParamEchoSync(string key1, int key2)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).GetAwaiter().GetResult();

            return $"{key1}_{key2}";
        }

        public async Task<IDictionary<int, string>> MultiParamEnumerableKey(string outerKey, IEnumerable<int> innerKeys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return innerKeys.ToDictionary(k => k, k => outerKey + k);
        }

        public async Task<string> StringToStringCanx(string key, CancellationToken token)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value, token);
            
            token.ThrowIfCancellationRequested();
            
            return key;
        }

        public async Task<IDictionary<string, string>> MultiEchoCanx(IEnumerable<string> keys, CancellationToken token)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value, token);

            token.ThrowIfCancellationRequested();
            
            return keys.ToDictionary(k => k);
        }
        
        public async Task<string> MultiParamEchoCanx(string key1, int key2, CancellationToken token)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value, token);

            token.ThrowIfCancellationRequested();
            
            return $"{key1}_{key2}";
        }

        public async Task<IDictionary<int, string>> MultiParamEnumerableKeyCanx(
            string outerKey,
            IEnumerable<int> innerKeys,
            CancellationToken token)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value, token);

            token.ThrowIfCancellationRequested();
            
            return innerKeys.ToDictionary(k => k, k => outerKey + k);
        }

        public string UnsupportedFunc(int a, int b, int c, int d, int e)
        {
            return $"{a}_{b}_{c}_{d}_{e}";
        }

        public void UnsupportedAction(int a)
        {
            if (a == 123)
                throw new Exception();
        }
        
        public int UnsupportedProperty { get; set; }
    }
}