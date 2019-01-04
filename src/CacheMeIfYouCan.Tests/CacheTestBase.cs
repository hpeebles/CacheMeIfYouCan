using System;
using System.Threading;

namespace CacheMeIfYouCan.Tests
{
    public abstract class CacheTestBase
    {
        private static readonly ReaderWriterLockSlim DefaultSettingsLock = new ReaderWriterLockSlim();

        protected IDisposable EnterSetup(bool willWriteToDefaultSettings)
        {
            if (willWriteToDefaultSettings)
                return new WriteLock();
            
            return new ReadLock();
        }

        private class ReadLock : IDisposable
        {
            public ReadLock()
            {
                DefaultSettingsLock.EnterReadLock();
            }

            public void Dispose()
            {
                DefaultSettingsLock.ExitReadLock();
            }
        }

        private class WriteLock : IDisposable
        {
            public WriteLock()
            {
                DefaultSettingsLock.EnterWriteLock();
            }

            public void Dispose()
            {
                DefaultSettingsLock.ExitWriteLock();
            }
        }
    }
}