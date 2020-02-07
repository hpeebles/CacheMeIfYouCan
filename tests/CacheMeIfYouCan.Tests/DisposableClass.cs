using System;

namespace CacheMeIfYouCan.Tests
{
    public class DisposableClass : IDisposable
    {
        public bool IsDisposed { get; private set; }
        
        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}