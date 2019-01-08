using System;
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
        private readonly Func<string, string> _stringToStringSync_5;
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            var methods = typeof(ITest).GetMethods();
            
            _stringToString_0 = new FunctionCacheConfigurationManager<string, string>(impl.StringToString, config, methods[0]).Build();
            _intToString_1 = new FunctionCacheConfigurationManager<int, string>(impl.IntToString, config, methods[1]).Build();
            _longToInt_2 = new FunctionCacheConfigurationManager<long, int>(impl.LongToInt, config, methods[2]).Build();
            _multiEcho_3 = new MultiKeyFunctionCacheConfigurationManager<string, string>(impl.MultiEcho, config, methods[3]).Build();
            _multiEchoList_4 = new MultiKeyFunctionCacheConfigurationManager<string, string>(impl.MultiEcho, config, methods[4]).Build();
            _stringToStringSync_5 = new FunctionCacheConfigurationManagerSync<string, string>(impl.StringToStringSync, config, methods[5]).Build();
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

        public string StringToStringSync(string key)
        {
            return _stringToStringSync_5(key);
        }
    }
}