namespace CacheMeIfYouCan.Internal
{
    internal readonly struct ValueWithTimestamp<T>
    {
        public ValueWithTimestamp(T value, long stopwatchTimestampCompleted)
        {
            Value = value;
            StopwatchTimestampCompleted = stopwatchTimestampCompleted;
        }
        
        public T Value { get; }
        public long StopwatchTimestampCompleted { get; }
    }
}