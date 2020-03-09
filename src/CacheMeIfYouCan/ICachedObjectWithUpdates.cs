using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan
{
    public interface ICachedObjectWithUpdates<T, TUpdateFuncInput> : ICachedObject<T>
    {
        void UpdateValue(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken = default);
        Task UpdateValueAsync(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken = default);
        event EventHandler<ValueUpdatedEvent<T, TUpdateFuncInput>> OnValueUpdated;
        event EventHandler<ValueUpdateExceptionEvent<T, TUpdateFuncInput>> OnValueUpdateException;
    }
}