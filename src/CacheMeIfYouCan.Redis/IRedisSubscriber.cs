using System;

namespace CacheMeIfYouCan.Redis
{
    public interface IRedisSubscriber
    {
        void SubscribeToKeyChanges(int dbIndex, KeyEvents keyEvents, Action<string, KeyEvents> onKeyChangedAction);
    }
}