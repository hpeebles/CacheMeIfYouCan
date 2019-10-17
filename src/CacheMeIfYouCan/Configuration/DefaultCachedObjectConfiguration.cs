using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal Action<CachedObjectSuccessfulUpdateResult> OnValueUpdatedAction { get; private set; }
        internal Action<CachedObjectUpdateException> OnExceptionAction { get; private set; }

        public DefaultCachedObjectConfiguration OnValueUpdated(
            Action<CachedObjectSuccessfulUpdateResult> onValueUpdated,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnValueUpdatedAction = ActionsHelper.Combine(OnValueUpdatedAction, onValueUpdated, behaviour);
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
