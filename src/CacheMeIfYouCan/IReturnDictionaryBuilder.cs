using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface IReturnDictionaryBuilder<in TK, in TV, out TDictionary>
        where TDictionary : IDictionary<TK, TV>
    {
        TDictionary BuildResponse(IEnumerable<IKeyValuePair<TK, TV>> values, int count);
    }
    
    public static class ReturnDictionaryBuilderExtensions
    {
        public static TDictionary BuildResponse<TK, TV, TDictionary>(
            this IReturnDictionaryBuilder<TK, TV, TDictionary> builder,
            IReadOnlyCollection<IKeyValuePair<TK, TV>> values)
            where TDictionary : IDictionary<TK, TV>
        {
            return builder.BuildResponse(values, values.Count);
        }
    }
}