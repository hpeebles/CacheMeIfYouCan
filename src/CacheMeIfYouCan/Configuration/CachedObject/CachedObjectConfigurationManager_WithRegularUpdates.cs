using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration.CachedObject
{
    public class CachedObjectConfigurationManager_WithRegularUpdates<T> : CachedObjectConfigurationManager<T, Unit>
    {
        private readonly TimeSpan _onSuccess;
        private readonly TimeSpan _onFailure;

        internal CachedObjectConfigurationManager_WithRegularUpdates(
            Func<Task<T>> getValueFunc,
            Func<T, Unit, Task<T>> updateValueFunc,
            TimeSpan onSuccess,
            TimeSpan onFailure)
            : base(getValueFunc, updateValueFunc, new CachedObjectRegularIntervalWithJitterScheduler<T>(result => result.Success ? onSuccess : onFailure, 0))
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
        }

        public CachedObjectConfigurationManager<T, Unit> WithJitter(double jitterPercentage)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                InitialiseValueFunc,
                UpdateValueFunc,
                new CachedObjectRegularIntervalWithJitterScheduler<T>(
                    result => result.Success
                        ? _onSuccess
                        : _onFailure,
                    jitterPercentage));
        }
    }
}