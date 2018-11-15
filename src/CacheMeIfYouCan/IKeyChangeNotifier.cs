using System;

namespace CacheMeIfYouCan
{
    public interface IKeyChangeNotifier<TK>
    {
        IObservable<Key<TK>> KeyChanges { get; }
    }
}