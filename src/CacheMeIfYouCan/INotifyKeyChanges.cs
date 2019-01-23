using System;

namespace CacheMeIfYouCan
{
    public interface INotifyKeyChanges<TK>
    {
        bool NotifyKeyChangesEnabled { get; }
        
        IObservable<Key<TK>> KeyChanges { get; }
    }
}