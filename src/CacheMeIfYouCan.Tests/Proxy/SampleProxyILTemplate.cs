using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
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
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            var methods = typeof(ITest).GetMethods();
            
            _stringToString_0 = new FunctionCacheConfigurationManager<string, string>(
                impl.StringToString, config, methods[0]).Build();
            
            _intToString_1 = new FunctionCacheConfigurationManager<int, string>(
                impl.IntToString, config, methods[1]).Build();
            
            _longToInt_2 = new FunctionCacheConfigurationManager<long, int>(
                impl.LongToInt, config, methods[2]).Build();
            
            _multiEcho_3 = new MultiKeyFunctionCacheConfigurationManager<IEnumerable<string>, IDictionary<string, string>, string, string>(
                impl.MultiEcho, config, methods[3]).Build();
            
            _multiEchoList_4 = new MultiKeyFunctionCacheConfigurationManager<IList<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoList, config, methods[4]).Build();
            
            _multiEchoSet_5 = new MultiKeyFunctionCacheConfigurationManager<ISet<string>, IDictionary<string, string>, string, string>(
                impl.MultiEchoSet, config, methods[4]).Build();
            
            _stringToStringSync_6 = new FunctionCacheConfigurationManagerSync<string, string>(
                impl.StringToStringSync, config, methods[5]).Build();
            
            _multiStringToStringSync_7 = new MultiKeyFunctionCacheConfigurationManagerSync<ICollection<string>, IDictionary<string, string>, string, string>(
                impl.MultiStringToStringSync, config, methods[6]).Build();
            
            _multiStringToConcurrent_8 = new MultiKeyFunctionCacheConfigurationManager<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>(
                impl.MultiEchoToConcurrent, config, methods[4]).Build();
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
    }
}