using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal Action<CachedObjectUpdateResult> OnUpdateAction { get; private set; }
        internal Action<CachedObjectUpdateException> OnExceptionAction { get; private set; }

        public DefaultCachedObjectConfiguration OnUpdate(Action<CachedObjectUpdateResult> onUpdate)
        {
            OnUpdateAction = onUpdate;
            return this;
        }

        public DefaultCachedObjectConfiguration OnException(Action<CachedObjectUpdateException> onException)
        {
            OnExceptionAction = onException;
            return this;
        }
    }
}
