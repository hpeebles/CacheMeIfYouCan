using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Tests.Proxy
{
    // Leave this class as a template for generating the IL within CachedProxyFactory
    internal class SampleProxyILTemplate : ITest
    {
        private readonly ITest _impl;
        private readonly Func<string, Task<string>> _stringToString_0;
        private readonly Func<int, Task<string>> _intToString_1;
        private readonly Func<long, Task<int>> _longToInt_2;
        private readonly Func<IEnumerable<string>, Task<IDictionary<string, string>>> _multiEcho_3;
        private readonly Func<IList<string>, Task<IDictionary<string, string>>> _multiEchoList_4;
        private readonly Func<ISet<string>, Task<IDictionary<string, string>>> _multiEchoSet_5;
        private readonly Func<string, string> _stringToStringSync_6;
        private readonly Func<ICollection<string>, IDictionary<string, string>> _multiStringToStringSync_7;
        private readonly Func<IEnumerable<string>, Task<ConcurrentDictionary<string, string>>> _multiStringToConcurrent_8;
        private readonly Func<string, int, Task<string>> _multiParamEcho_9;
        private readonly Func<string, int, string> _multiParamEchoSync_10;
        private readonly Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> _multiParamEnumerableKey_11;
        private readonly Func<string, CancellationToken, Task<string>> _stringToStringCanx_12;
        private readonly Func<IEnumerable<string>, CancellationToken, Task<IDictionary<string, string>>> _multiEchoCanx_13;
        private readonly Func<string, int, CancellationToken, Task<string>> _multiParamEchoCanx_14;
        private readonly Func<string, IEnumerable<int>, CancellationToken, Task<IDictionary<int, string>>> _multiParamEnumerableKeyCanx_15;
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            _impl = impl;
            
            var methods = typeof(ITest).GetMethods();
            
            _stringToString_0 = new SingleKeyFunctionCacheConfigurationManagerNoCanx<string, string>(
                impl.StringToString, config, methods[0]).Build();
            
            _intToString_1 = new SingleKeyFunctionCacheConfigurationManagerNoCanx<int, string>(
                impl.IntToString, config, methods[1]).Build();
            
            _longToInt_2 = new SingleKeyFunctionCacheConfigurationManagerNoCanx<long, int>(
                impl.LongToInt, config, methods[2]).Build();
            
            _multiEcho_3 = new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, IDictionary<string, string>, string, string>(
                impl.MultiEcho, config, methods[3]).Build();
            
            _multiEchoList_4 = new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IList<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoList, config, methods[4]).Build();
            
            _multiEchoSet_5 = new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ISet<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoSet, config, methods[5]).Build();
            
            _stringToStringSync_6 = new SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<string, string>(
                impl.StringToStringSync, config, methods[6]).Build();
            
            _multiStringToStringSync_7 = new EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ICollection<string>, IDictionary<string, string>, string, string>(
                impl.MultiStringToStringSync, config, methods[7]).Build();
            
            _multiStringToConcurrent_8 = new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>(
                impl.MultiEchoToConcurrent, config, methods[8]).Build();
            
            _multiParamEcho_9 = new MultiParamFunctionCacheConfigurationManagerNoCanx<string, int, string>(
                impl.MultiParamEcho, config, methods[9]).Build();
            
            _multiParamEchoSync_10 = new MultiParamFunctionCacheConfigurationManagerSyncNoCanx<string, int, string>(
                impl.MultiParamEchoSync, config, methods[10]).Build();
            
            _multiParamEnumerableKey_11 = new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<string, IEnumerable<int>, IDictionary<int, string>, int, string>(
                impl.MultiParamEnumerableKey, config, methods[11]).Build();
            
            _stringToStringCanx_12 = new SingleKeyFunctionCacheConfigurationManagerCanx<string, string>(
                impl.StringToStringCanx, config, methods[12]).Build();
            
            _multiEchoCanx_13 = new EnumerableKeyFunctionCacheConfigurationManagerCanx<IEnumerable<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoCanx, config, methods[3]).Build();

            _multiParamEchoCanx_14 = new MultiParamFunctionCacheConfigurationManagerCanx<string, int, string>(
                impl.MultiParamEchoCanx, config, methods[9]).Build();
            
            _multiParamEnumerableKeyCanx_15 = new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<string, IEnumerable<int>, IDictionary<int, string>, int, string>(
                impl.MultiParamEnumerableKeyCanx, config, methods[11]).Build();
        }
        
        public Task<string> StringToString(string key)
        {
            return _stringToString_0(key);
        }

        public Task<string> IntToString(int key)
        {
            return _intToString_1(key);
        }

        public Task<int> LongToInt(long key)
        {
            return _longToInt_2(key);
        }
        
        public Task<IDictionary<string, string>> MultiEcho(IEnumerable<string> keys)
        {
            return _multiEcho_3(keys);
        }
        
        public Task<IDictionary<string, string>> MultiEchoList(IList<string> keys)
        {
            return _multiEchoList_4(keys);
        }
        
        public Task<IDictionary<string, string>> MultiEchoSet(ISet<string> keys)
        {
            return _multiEchoSet_5(keys);
        }

        public string StringToStringSync(string key)
        {
            return _stringToStringSync_6(key);
        }

        public IDictionary<string, string> MultiStringToStringSync(ICollection<string> keys)
        {
            return _multiStringToStringSync_7(keys);
        }
        
        public Task<ConcurrentDictionary<string, string>> MultiEchoToConcurrent(IEnumerable<string> keys)
        {
            return _multiStringToConcurrent_8(keys);
        }

        public Task<string> MultiParamEcho(string key1, int key2)
        {
            return _multiParamEcho_9(key1, key2);
        }

        public string MultiParamEchoSync(string key1, int key2)
        {
            return _multiParamEchoSync_10(key1, key2);
        }

        public Task<IDictionary<int, string>> MultiParamEnumerableKey(string outerKey, IEnumerable<int> innerKeys)
        {
            return _multiParamEnumerableKey_11(outerKey, innerKeys);
        }

        public Task<string> StringToStringCanx(string key, CancellationToken token)
        {
            return _stringToStringCanx_12(key, token);
        }
        
        public Task<IDictionary<string, string>> MultiEchoCanx(IEnumerable<string> keys, CancellationToken token)
        {
            return _multiEchoCanx_13(keys, token);
        }

        public Task<string> MultiParamEchoCanx(string key1, int key2, CancellationToken token)
        {
            return _multiParamEchoCanx_14(key1, key2, token);
        }

        public Task<IDictionary<int, string>> MultiParamEnumerableKeyCanx(string outerKey, IEnumerable<int> innerKeys, CancellationToken token)
        {
            return _multiParamEnumerableKeyCanx_15(outerKey, innerKeys, token);
        }

        public string UnsupportedFunc(int a, int b, int c, int d, int e)
        {
            return _impl.UnsupportedFunc(a, b, c, d, e);
        }

        public void UnsupportedAction(int a)
        {
            _impl.UnsupportedAction(a);
        }

        public int UnsupportedProperty
        {
            get => _impl.UnsupportedProperty;
            set => _impl.UnsupportedProperty = value;
        }
    }
}