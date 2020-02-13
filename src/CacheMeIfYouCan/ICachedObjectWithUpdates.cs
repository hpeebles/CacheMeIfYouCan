using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObjectWithUpdates<T, TUpdateFuncInput> : ICachedObject<T>
    {
        void UpdateValue(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken = default);
        
        Task UpdateValueAsync(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken = default);
        
        event EventHandler<CachedObjectValueUpdatedEvent<T, TUpdateFuncInput>> OnValueUpdated;
        
        event EventHandler<CachedObjectValueUpdateExceptionEvent<T, TUpdateFuncInput>> OnValueUpdateException;
    }
}