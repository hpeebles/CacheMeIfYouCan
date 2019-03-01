using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Common
{
    public class EchoSync
    {
        private readonly Func<string, TimeSpan> _delayFunc;
        private readonly Func<string, bool> _errorFunc;
        
        public EchoSync()
            : this(TimeSpan.Zero)
        { }
        
        public EchoSync(TimeSpan delay)
            : this(k => delay)
        { }
        
        public EchoSync(Func<string, TimeSpan> delayFunc)
            : this(delayFunc, k => false)
        { }
        
        public EchoSync(TimeSpan delay, Func<string, bool> errorFunc)
            : this(k => delay, errorFunc)
        { }
        
        public EchoSync(Func<string, TimeSpan> delayFunc, Func<string, bool> errorFunc)
        {
            _delayFunc = delayFunc;
            _errorFunc = errorFunc;
        }

        private string Call(string key, CancellationToken token)
        {
            Task.Delay(_delayFunc(key), token).GetAwaiter().GetResult();
            
            if (_errorFunc(key))
                throw new Exception();

            return key;
        }

        public static implicit operator Func<string, string>(EchoSync echo)
        {
            return k => echo.Call(k, CancellationToken.None);
        }
        
        public static implicit operator Func<string, CancellationToken, string>(EchoSync echo)
        {
            return echo.Call;
        }
    }
}