using System;

namespace CacheMeIfYouCan.Redis
{
    public interface IRedisSubscriber : IDisposable
    {
        void SubscribeToKeyChanges(
            int dbIndex,
            KeyEventType keyEventTypes,
            Action<string, KeyEventType> onKeyChangedAction,
            string keyPrefix = null);
    }
}