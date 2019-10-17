using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class ActionsHelper
    {
        public static Action<T> Combine<T>(Action<T> current, Action<T> action, AdditionBehaviour behaviour)
        {
            // Can't use 'return current + action' since the types can be different due to Action's contravariance
            if (current == null || behaviour == AdditionBehaviour.Overwrite)
                return action;

            if (action == null)
                return current;

            if (behaviour == AdditionBehaviour.Append)
                return x => { current(x); action(x); };

            return x => { action(x); current(x); };
        }
    }
}