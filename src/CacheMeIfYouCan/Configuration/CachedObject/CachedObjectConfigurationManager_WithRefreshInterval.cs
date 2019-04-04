using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration.CachedObject
{
    public class CachedObjectConfigurationManager_WithRefreshInterval<T> : CachedObjectConfigurationManager<T, Unit>
    {
        private readonly TimeSpan _onSuccess;
        private readonly TimeSpan _onFailure;

        internal CachedObjectConfigurationManager_WithRefreshInterval(
            Func<Task<T>> getValueFunc,
            TimeSpan onSuccess,
            TimeSpan onFailure)
            : base(getValueFunc, null, new CachedObjectRefreshWithJitterScheduler<T>(result => result.Success ? onSuccess : onFailure, 0))
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
        }

        public CachedObjectConfigurationManager<T, Unit> WithJitter(double jitterPercentage)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                InitialiseValueFunc,
                null,
                new CachedObjectRefreshWithJitterScheduler<T>(result => result.Success ? _onSuccess : _onFailure, jitterPercentage));
        }
    }
}