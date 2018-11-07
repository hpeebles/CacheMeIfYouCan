using System;

namespace CacheMeIfYouCan.Caches
{
    public interface IKeyChangeNotifier<TK>
    {
        IObservable<Key<TK>> KeyChanges { get; }
    }
}