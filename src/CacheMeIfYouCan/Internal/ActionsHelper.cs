using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class ActionsHelper
    {
        public static Action<T> Combine<T>(Action<T> current, Action<T> action, AdditionBehaviour behaviour)
        {
            switch (behaviour)
            {
                case AdditionBehaviour.Overwrite:
                    return action;
                
                case AdditionBehaviour.Prepend:
                    return action + current;
                
                default:
                    return current + action;
            }
        }
    }
}