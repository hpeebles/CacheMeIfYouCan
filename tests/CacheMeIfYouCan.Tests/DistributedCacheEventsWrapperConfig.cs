using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests
{
    public sealed class DistributedCacheEventsWrapperConfig<TKey, TValue>
    {
        public Action<TKey, bool, TValue, TimeSpan> OnTryGetCompletedSuccessfully { get; set; }
        public Func<TKey, TimeSpan, Exception, bool> OnTryGetException { get; set; }
        public Action<TKey, TValue, TimeSpan, TimeSpan> OnSetCompletedSuccessfully { get; set; }
        public Func<TKey, TValue, TimeSpan, TimeSpan, Exception, bool> OnSetException { get; set; }
        public Action<IReadOnlyCollection<TKey>, IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>, TimeSpan> OnGetManyCompletedSuccessfully { get; set; }
        public Func<IReadOnlyCollection<TKey>, TimeSpan, Exception, bool> OnGetManyException { get; set; }
        public Action<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan> OnSetManyCompletedSuccessfully { get; set; }
        public Func<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> OnSetManyException { get; set; }
        public Action<TKey, bool, TimeSpan> OnTryRemoveCompletedSuccessfully { get; set; }
        public Func<TKey, TimeSpan, Exception, bool> OnTryRemoveException { get; set; }
    }
    
    public sealed class DistributedCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue>
    {
        public Action<TOuterKey, IReadOnlyCollection<TInnerKey>, IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan> OnGetManyCompletedSuccessfully { get; set; }
        public Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan, Exception, bool> OnGetManyException { get; set; }
        public Action<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan> OnSetManyCompletedSuccessfully { get; set; }
        public Func<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> OnSetManyException { get; set; }
        public Action<TOuterKey, TInnerKey, bool, TimeSpan> OnTryRemoveCompletedSuccessfully { get; set; }
        public Func<TOuterKey, TInnerKey, TimeSpan, Exception, bool> OnTryRemoveException { get; set; }
    }
}