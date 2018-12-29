using System;

namespace CacheMeIfYouCan
{
    public interface INotifyKeyChanges<TK>
    {
        IObservable<Key<TK>> KeyChanges { get; }
    }
}