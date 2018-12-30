namespace CacheMeIfYouCan
{
    public readonly struct PendingRequestsCount
    {
        internal PendingRequestsCount(string name, string type, int count)
        {
            Name = name;
            Type = type;
            Count = count;
        }
        
        public string Name { get; }
        public string Type { get; }
        public int Count { get; }
    }
}