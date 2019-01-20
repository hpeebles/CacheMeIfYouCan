using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Tests.Proxy
{
    // Leave this class as a template for generating the IL within CachedProxyFactory
    internal class SampleProxyILTemplate : ITest
    {
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
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            var methods = typeof(ITest).GetMethods();
            
            _stringToString_0 = new SingleKeyFunctionCacheConfigurationManager<string, string>(
                impl.StringToString, config, methods[0]).Build();
            
            _intToString_1 = new SingleKeyFunctionCacheConfigurationManager<int, string>(
                impl.IntToString, config, methods[1]).Build();
            
            _longToInt_2 = new SingleKeyFunctionCacheConfigurationManager<long, int>(
                impl.LongToInt, config, methods[2]).Build();
            
            _multiEcho_3 = new EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, IDictionary<string, string>, string, string>(
                impl.MultiEcho, config, methods[3]).Build();
            
            _multiEchoList_4 = new EnumerableKeyFunctionCacheConfigurationManager<IList<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoList, config, methods[4]).Build();
            
            _multiEchoSet_5 = new EnumerableKeyFunctionCacheConfigurationManager<ISet<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoSet, config, methods[5]).Build();
            
            _stringToStringSync_6 = new SingleKeyFunctionCacheConfigurationManagerSync<string, string>(
                impl.StringToStringSync, config, methods[6]).Build();
            
            _multiStringToStringSync_7 = new EnumerableKeyFunctionCacheConfigurationManagerSync<ICollection<string>, IDictionary<string, string>, string, string>(
                impl.MultiStringToStringSync, config, methods[7]).Build();
            
            _multiStringToConcurrent_8 = new EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>(
                impl.MultiEchoToConcurrent, config, methods[8]).Build();
            
            _multiParamEcho_9 = new MultiParamFunctionCacheConfigurationManager<string, int, string>(
                impl.MultiParamEcho, config, methods[9]).Build();
            
            _multiParamEchoSync_10 = new MultiParamFunctionCacheConfigurationManagerSync<string, int, string>(
                impl.MultiParamEchoSync, config, methods[10]).Build();
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
    }
}