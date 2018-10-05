using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests
{
    public class Echo
    {
        private readonly Func<string, TimeSpan> _delayFunc;
        private readonly Func<string, bool> _errorFunc;
        
        public Echo()
            : this(TimeSpan.Zero)
        { }
        
        public Echo(TimeSpan delay)
            : this(k => delay)
        { }
        
        public Echo(Func<string, TimeSpan> delayFunc)
            : this(delayFunc, k => false)
        { }
        
        public Echo(TimeSpan delay, Func<string, bool> errorFunc)
            : this(k => delay, errorFunc)
        { }
        
        public Echo(Func<string, TimeSpan> delayFunc, Func<string, bool> errorFunc)
        {
            _delayFunc = delayFunc;
            _errorFunc = errorFunc;
        }

        private async Task<string> Call(string key)
        {
            await Task.Delay(_delayFunc(key));
            
            if (_errorFunc(key))
                throw new Exception();

            return key;
        }

        public static implicit operator Func<string, Task<string>>(Echo echo)
        {
            return echo.Call;
        }
    }
}