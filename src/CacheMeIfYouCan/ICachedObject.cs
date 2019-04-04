using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<out T> : ICachedObjectInitializer, IDisposable
    {
        T Value { get; }
    }

    public interface ICachedObjectInitializer
    {
        string Name { get; }
        
        Task<CachedObjectInitializeOutcome> Initialize();
    }
}