using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Proxy
{
    // Leave this class as a template for generating the IL within CachedProxyFactory
    internal class SampleProxyILTemplate : ITest
    {
        private readonly Func<string, Task<string>> _stringToString;
        private readonly Func<int, Task<string>> _intToString;
        private readonly Func<double, Task<double>> _doubleToDouble;
        
        public SampleProxyILTemplate(ITest impl, CachedProxyConfig config)
        {
            _stringToString = new FunctionCacheConfigurationManager<string, string>(impl.StringToString, "StringToString", config).Build();
            _intToString = new FunctionCacheConfigurationManager<int, string>(impl.IntToString, "IntToString", config).Build();
            _doubleToDouble = new FunctionCacheConfigurationManager<double, double>(impl.DoubleToDouble, "DoubleToDouble", config).Build();
        }
        
        public Task<string> StringToString(string key)
        {
            return _stringToString(key);
        }

        public Task<string> IntToString(int key)
        {
            return _intToString(key);
        }

        public Task<double> DoubleToDouble(double key)
        {
            return _doubleToDouble(key);
        }
    }
}