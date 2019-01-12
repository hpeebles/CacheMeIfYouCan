using System;
using CacheMeIfYouCan.Tests.Helpers;

namespace CacheMeIfYouCan.Tests
{
    public class CachedObjectSetupLock
    {
        private static readonly ReadWriteLock DefaultSettingsLock = new ReadWriteLock();

        public IDisposable Enter(bool willWriteToDefaultSettings = false)
        {
            return willWriteToDefaultSettings
                ? DefaultSettingsLock.CreateReadLock()
                : DefaultSettingsLock.CreateWriteLock();
        }
    }
}