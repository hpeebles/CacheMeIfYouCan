using System;

namespace CacheMeIfYouCan.Configuration
{
    public static class ActionsHelper
    {
        public static Action<T> Combine<T>(Action<T> current, Action<T> action, ActionOrdering ordering)
        {
            if (current == null || ordering == ActionOrdering.Overwrite)
                return action;

            if (action == null)
                return current;

            if (ordering == ActionOrdering.Append)
                return x => { current(x); action(x); };
            
            return x => { action(x); current(x); };
        }
    }
}