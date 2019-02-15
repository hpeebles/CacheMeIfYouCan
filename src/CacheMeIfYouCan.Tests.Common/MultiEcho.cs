using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Common
{
    public class MultiEcho
    {
        private readonly Func<TimeSpan> _delayFunc;
        private readonly Func<bool> _errorFunc;
        
        public MultiEcho()
            : this(TimeSpan.Zero)
        { }
        
        public MultiEcho(TimeSpan delay)
            : this(delay, () => false)
        { }
        
        public MultiEcho(TimeSpan delay, Func<bool> errorFunc)
            : this(() => delay, errorFunc)
        { }
        
        public MultiEcho(Func<TimeSpan> delayFunc, Func<bool> errorFunc)
        {
            _delayFunc = delayFunc;
            _errorFunc = errorFunc;
        }

        private async Task<IDictionary<string, string>> Call(IEnumerable<string> keys)
        {
            await Task.Delay(_delayFunc());
            
            if (_errorFunc())
                throw new Exception();

            return keys.ToDictionary(k => k);
        }

        public static implicit operator Func<IEnumerable<string>, Task<IDictionary<string, string>>>(MultiEcho echo)
        {
            return echo.Call;
        }
    }
}