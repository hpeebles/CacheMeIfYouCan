using System;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    public class SynchronizationContextRemover : IDisposable
    {
        private readonly SynchronizationContext _context;

        private SynchronizationContextRemover(SynchronizationContext context)
        {
            _context = context;
        }

        public static SynchronizationContextRemover StartNew()
        {
            var context = SynchronizationContext.Current;
            
            if (context != null)
                SynchronizationContext.SetSynchronizationContext(null);
            
            return new SynchronizationContextRemover(context);
        }

        public void Dispose()
        {
            if (_context != null)
                SynchronizationContext.SetSynchronizationContext(_context);
        }
    }
}