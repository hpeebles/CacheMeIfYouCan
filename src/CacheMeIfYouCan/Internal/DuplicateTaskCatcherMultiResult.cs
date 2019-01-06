namespace CacheMeIfYouCan.Internal
{
    internal readonly struct DuplicateTaskCatcherMultiResult<TK, TV>
    {
        public DuplicateTaskCatcherMultiResult(TK key, TV value, bool duplicate, long stopwatchTimestampCompleted)
        {
            Key = key;
            Value = value;
            Duplicate = duplicate;
            StopwatchTimestampCompleted = stopwatchTimestampCompleted;
        }
        
        public TK Key { get; }
        public TV Value { get; }
        public bool Duplicate { get; }
        public long StopwatchTimestampCompleted { get; }
    }
}