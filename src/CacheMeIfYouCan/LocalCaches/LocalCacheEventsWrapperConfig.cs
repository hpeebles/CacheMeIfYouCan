using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.LocalCaches
{
    public sealed class LocalCacheEventsWrapperConfig<TKey, TValue>
    {
        public Action<TKey, bool, TValue, TimeSpan> OnTryGetCompletedSuccessfully { get; set; }
        public Func<TKey, TimeSpan, Exception, bool> OnTryGetException { get; set; }
        public Action<TKey, TValue, TimeSpan, TimeSpan> OnSetCompletedSuccessfully { get; set; }
        public Func<TKey, TValue, TimeSpan, TimeSpan, Exception, bool> OnSetException { get; set; }
        public Action<IReadOnlyCollection<TKey>, Memory<KeyValuePair<TKey, TValue>>, TimeSpan> OnGetManyCompletedSuccessfully { get; set; }
        public Func<IReadOnlyCollection<TKey>, TimeSpan, Exception, bool> OnGetManyException { get; set; }
        public Action<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan> OnSetManyCompletedSuccessfully { get; set; }
        public Func<IReadOnlyCollection<KeyValuePair<TKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> OnSetManyException { get; set; }
        public Action<TKey, bool, TValue, TimeSpan> OnTryRemoveCompletedSuccessfully { get; set; }
        public Func<TKey, TimeSpan, Exception, bool> OnTryRemoveException { get; set; }
    }
    
    public sealed class LocalCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue>
    {
        public Action<TOuterKey, IReadOnlyCollection<TInnerKey>, Memory<KeyValuePair<TInnerKey, TValue>>, TimeSpan> OnGetManyCompletedSuccessfully { get; set; }
        public Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan, Exception, bool> OnGetManyException { get; set; }
        public Action<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan> OnSetManyCompletedSuccessfully { get; set; }
        public Func<TOuterKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>, TimeSpan, TimeSpan, Exception, bool> OnSetManyException { get; set; }
        public Action<TOuterKey, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan> OnSetManyWithVaryingTimesToLiveCompletedSuccessfully { get; set; }
        public Func<TOuterKey, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>, TimeSpan, Exception, bool> OnSetManyWithVaryingTimesToLiveException { get; set; }
        public Action<TOuterKey, TInnerKey, bool, TValue, TimeSpan> OnTryRemoveCompletedSuccessfully { get; set; }
        public Func<TOuterKey, TInnerKey, TimeSpan, Exception, bool> OnTryRemoveException { get; set; }
    }
}