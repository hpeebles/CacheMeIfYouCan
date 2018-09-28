namespace CacheMeIfYouCan.Internal
{
    internal interface IKeySetFactory<TK>
    {
        IKeySet<TK> New();
    }
    
    internal class StringKeySetFactory<TK> : IKeySetFactory<TK>
    {
        public IKeySet<TK> New()
        {
            return new StringKeySet<TK>();
        }
    }
    
    internal class GenericKeySetFactory<TK> : IKeySetFactory<TK>
    {
        public IKeySet<TK> New()
        {
            return new GenericKeySet<TK>();
        }
    }
}