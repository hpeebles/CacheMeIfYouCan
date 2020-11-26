namespace CacheMeIfYouCan.Internal
{
    internal sealed class NullObj
    {
        private NullObj() { }
            
        public static NullObj Instance { get; } = new NullObj();
    }
}