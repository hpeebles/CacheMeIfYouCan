using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Tests.Proxy
{
    // Leave this class as a template for generating the IL within CachedProxyFactory
    internal class SampleProxyILTemplate : ITest
    {
        private readonly Func<string, Task<string>> _stringToString;
        private readonly Func<int, Task<string>> _intToString;
        private readonly Func<long, Task<int>> _longToInt;
        private readonly Func<IEnumerable<string>, Task<IDictionary<string, string>>> _multiEcho;
        private readonly Func<IList<string>, Task<IDictionary<string, string>>> _multiEchoList;
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            _stringToString = new FunctionCacheConfigurationManager<string, string>(impl.StringToString, "StringToString", config).Build();
            _intToString = new FunctionCacheConfigurationManager<int, string>(impl.IntToString, "IntToString", config).Build();
            _longToInt = new FunctionCacheConfigurationManager<long, int>(impl.LongToInt, "LongToInt", config).Build();
            _multiEcho = new MultiKeyFunctionCacheConfigurationManager<string, string>(impl.MultiEcho, "MultiEcho", config).Build();
            _multiEchoList = new MultiKeyFunctionCacheConfigurationManager<string, string>(impl.MultiEcho, "MultiEchoList", config).Build();
        }
        
        public Task<string> StringToString(string key)
        {
            return _stringToString(key);
        }

        public Task<string> IntToString(int key)
        {
            return _intToString(key);
        }

        public Task<int> LongToInt(long key)
        {
            return _longToInt(key);
        }
        
        public Task<IDictionary<string, string>> MultiEcho(IEnumerable<string> keys)
        {
            return _multiEcho(keys);
        }
        
        public Task<IDictionary<string, string>> MultiEchoList(IList<string> keys)
        {
            return _multiEchoList(keys);
        }
    }
}