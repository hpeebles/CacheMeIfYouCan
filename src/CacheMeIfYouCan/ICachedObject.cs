using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<out T> : IDisposable
    {
        T Value { get; }

        Task<bool> Init();
    }
}