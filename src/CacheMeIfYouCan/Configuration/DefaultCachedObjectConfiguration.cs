using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal Action<CachedObjectUpdateResult> OnUpdateAction { get; private set; }
        internal Action<CachedObjectUpdateException> OnExceptionAction { get; private set; }

        public DefaultCachedObjectConfiguration OnUpdate(
            Action<CachedObjectUpdateResult> onUpdate,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnUpdateAction = ActionsHelper.Combine(OnUpdateAction, onUpdate, behaviour);
            return this;
        }

        public DefaultCachedObjectConfiguration OnException(
            Action<CachedObjectUpdateException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnExceptionAction = ActionsHelper.Combine(OnExceptionAction, onException, behaviour);
            return this;
        }
    }
}
