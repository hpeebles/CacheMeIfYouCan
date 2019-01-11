using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    public static class MultiKeyFuncConverter
    {
        public static Func<IEnumerable<TK>, Task<TRes>> ConvertInput<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            var keysConverterFunc = GetKeysConverterFunc<TReq, TK>();

            return k => func(keysConverterFunc(k));
        }
        
        public static Func<IEnumerable<TK>, TRes> ConvertInput<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            var keysConverterFunc = GetKeysConverterFunc<TReq, TK>();

            return k => func(keysConverterFunc(k));
        }
        
        public static Func<TReq, Task<IDictionary<TK, TV>>> ConvertOutput<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return async k => await func(k);
        }
        
        public static Func<TReq, Task<TRes>> ConvertToAsync<TReq, TRes>(
            this Func<TReq, TRes> func)
        {
            return k => Task.Run(() => func(k));
        }

        private static Func<IEnumerable<TK>, TReq> GetKeysConverterFunc<TReq, TK>()
            where TReq : IEnumerable<TK>
        {
            var type = typeof(TReq);

            if (type == typeof(IEnumerable<TK>))
                return k => (TReq)k;
            
            if (type == typeof(IList<TK>))
                return k => (TReq)k.AsIList();

            if (type == typeof(List<TK>))
                return k => (TReq)(IList<TK>)k.AsList();

            if (type == typeof(TK[]))
                return k => (TReq)(IList<TK>)k.AsArray();

            if (type == typeof(ICollection<TK>))
                return k => (TReq)k.AsICollection();

            if (type == typeof(ISet<TK>))
                return k => (TReq)k.AsISet();

            if (type == typeof(HashSet<TK>))
                return k => (TReq)(ISet<TK>)k.AsHashSet();

            if (type == typeof(SortedSet<TK>))
                return k => (TReq)(ISet<TK>)k.AsSortedSet();
            
            throw new Exception($"Unsupported key type: '{type.Name}'");
        }
    }
}