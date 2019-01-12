using System;
using System.Threading;

namespace CacheMeIfYouCan.Tests.Helpers
{
    public class ReadWriteLock
    {
        private readonly ReaderWriterLockSlim _defaultSettingsLock = new ReaderWriterLockSlim();
        
        public IDisposable CreateReadLock() => new ReadLock(this);
        public IDisposable CreateWriteLock() => new WriteLock(this);
        
        private class ReadLock : IDisposable
        {
            private readonly ReadWriteLock _parent;

            public ReadLock(ReadWriteLock parent)
            {
                _parent = parent;
                _parent._defaultSettingsLock.EnterReadLock();
            }

            public void Dispose()
            {
                _parent._defaultSettingsLock.ExitReadLock();
            }
        }

        private class WriteLock : IDisposable
        {
            private readonly ReadWriteLock _parent;

            public WriteLock(ReadWriteLock parent)
            {
                _parent = parent;
                _parent._defaultSettingsLock.EnterWriteLock();
            }

            public void Dispose()
            {
                _parent._defaultSettingsLock.ExitWriteLock();
            }
        }
    }
}