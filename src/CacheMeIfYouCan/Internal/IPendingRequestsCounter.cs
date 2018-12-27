using System;

namespace CacheMeIfYouCan.Internal
{
    internal interface IPendingRequestsCounter : IDisposable
    {
        string Name { get; }
        string Type { get; }
        int PendingRequestsCount { get; }
    }
}