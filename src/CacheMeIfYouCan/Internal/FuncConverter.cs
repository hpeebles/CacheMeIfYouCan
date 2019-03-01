using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    public static class FuncConverter
    {
        public static Func<IEnumerable<TK>, CancellationToken, Task<TRes>> ConvertInputToEnumerable<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            var keysConverterFunc = GetKeysConverterFunc<TReq, TK>();

            return (k, t) => func(keysConverterFunc(k), t);
        }
        
        public static Func<TReq, CancellationToken, Task<TRes>> ConvertInputFromEnumerable<TReq, TRes, TK, TV>(
            this Func<IEnumerable<TK>, CancellationToken, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return (k, t) => func(k, t);
        }
        
        public static Func<TK1, IEnumerable<TK2>, CancellationToken, Task<TRes>> ConvertInputToEnumerable<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            var keysConverterFunc = GetKeysConverterFunc<TK2Enum, TK2>();

            return (k1, k2, t) => func(k1, keysConverterFunc(k2), t);
        }
        
        public static Func<TK1, TK2Enum, CancellationToken, Task<TRes>> ConvertInputFromEnumerable<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, IEnumerable<TK2>, CancellationToken, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return (k1, k2, t) => func(k1, k2, t);
        }
        
        public static Func<TReq, CancellationToken, Task<IDictionary<TK, TV>>> ConvertOutputToDictionary<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return async (k, t) => await func(k, t);
        }
        
        public static Func<TReq, CancellationToken, Task<TRes>> ConvertOutputFromDictionary<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, Task<IDictionary<TK, TV>>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return async (k, t) => (TRes)await func(k, t);
        }
        
        public static Func<TK1, TK2Enum, CancellationToken, Task<IDictionary<TK2, TV>>> ConvertOutputToDictionary<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return async (k1, k2, t) => await func(k1, k2, t);
        }
        
        public static Func<TK1, TK2Enum, CancellationToken, Task<TRes>> ConvertOutputFromDictionary<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, Task<IDictionary<TK2, TV>>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return async (k1, k2, t) => (TRes)await func(k1, k2, t);
        }
        
        public static Func<TReq, Task<TRes>> ConvertToAsync<TReq, TRes>(
            this Func<TReq, TRes> func)
        {
            return k => Task.Run(() => func(k));
        }
        
        public static Func<TReq1, TReq2, Task<TRes>> ConvertToAsync<TReq1, TReq2, TRes>(
            this Func<TReq1, TReq2, TRes> func)
        {
            return (r1, r2) => Task.Run(() => func(r1, r2));
        }
        
        public static Func<TReq1, TReq2, TReq3, Task<TRes>> ConvertToAsync<TReq1, TReq2, TReq3, TRes>(
            this Func<TReq1, TReq2, TReq3, TRes> func)
        {
            return (r1, r2, r3) => Task.Run(() => func(r1, r2, r3));
        }
        
        public static Func<TReq1, TReq2, TReq3, TReq4, Task<TRes>> ConvertToAsync<TReq1, TReq2, TReq3, TReq4, TRes>(
            this Func<TReq1, TReq2, TReq3, TReq4, TRes> func)
        {
            return (r1, r2, r3, r4) => Task.Run(() => func(r1, r2, r3, r4));
        }
        
        public static Func<TReq, CancellationToken, Task<TRes>> ConvertToAsync<TReq, TRes>(
            this Func<TReq, CancellationToken, TRes> func)
        {
            return (k, t) => Task.Run(() => func(k, t));
        }
        
        public static Func<TReq1, TReq2, CancellationToken, Task<TRes>> ConvertToAsync<TReq1, TReq2, TRes>(
            this Func<TReq1, TReq2, CancellationToken, TRes> func)
        {
            return (r1, r2, t) => Task.Run(() => func(r1, r2, t));
        }
        
        public static Func<TReq1, TReq2, TReq3, CancellationToken, Task<TRes>> ConvertToAsync<TReq1, TReq2, TReq3, TRes>(
            this Func<TReq1, TReq2, TReq3, CancellationToken, TRes> func)
        {
            return (r1, r2, r3, t) => Task.Run(() => func(r1, r2, r3, t));
        }
        
        public static Func<TReq1, TReq2, TReq3, TReq4, CancellationToken, Task<TRes>> ConvertToAsync<TReq1, TReq2, TReq3, TReq4, TRes>(
            this Func<TReq1, TReq2, TReq3, TReq4, CancellationToken, TRes> func)
        {
            return (r1, r2, r3, r4, t) => Task.Run(() => func(r1, r2, r3, r4, t));
        }
        
        public static Func<TReq, TRes> ConvertToSync<TReq, TRes>(
            this Func<TReq, Task<TRes>> func)
        {
            return x => Task.Run(() => func(x)).GetAwaiter().GetResult();
        }

        public static Func<TReq, CancellationToken, TRes> ConvertToSync<TReq, TRes>(
            this Func<TReq, CancellationToken, Task<TRes>> func)
        {
            return (x, t) => Task.Run(() => func(x, t)).GetAwaiter().GetResult();
        }
        
        public static Func<TK1, TK2, CancellationToken, TRes> ConvertToSync<TK1, TK2, TRes>(
            this Func<TK1, TK2, CancellationToken, Task<TRes>> func)
        {
            return (k1, k2, t) => Task.Run(() => func(k1, k2, t)).GetAwaiter().GetResult();
        }

        public static Func<(TK1, TK2), CancellationToken, TV> ConvertToSingleParam<TK1, TK2, TV>(
            this Func<TK1, TK2, CancellationToken, TV> func)
        {
            return (x, t) => func(x.Item1, x.Item2, t);
        }

        public static Func<(TK1, TK2, TK3), CancellationToken, TV> ConvertToSingleParam<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, CancellationToken, TV> func)
        {
            return (x, t) => func(x.Item1, x.Item2, x.Item3, t);
        }
        
        public static Func<(TK1, TK2, TK3, TK4), CancellationToken, TV> ConvertToSingleParam<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, CancellationToken, TV> func)
        {
            return (x, t) => func(x.Item1, x.Item2, x.Item3, x.Item4, t);
        }
        
        public static Func<TK1, TK2, CancellationToken, TV> ConvertToMultiParam<TK1, TK2, TV>(
            this Func<(TK1, TK2), CancellationToken, TV> func)
        {
            return (k1, k2, t) => func((k1, k2), t);
        }

        public static Func<TK1, TK2, TK3, CancellationToken, TV> ConvertToMultiParam<TK1, TK2, TK3, TV>(
            this Func<(TK1, TK2, TK3), CancellationToken, TV> func)
        {
            return (k1, k2, k3, t) => func((k1, k2, k3), t);
        }
        
        public static Func<TK1, TK2, TK3, TK4, CancellationToken, TV> ConvertToMultiParam<TK1, TK2, TK3, TK4, TV>(
            this Func<(TK1, TK2, TK3, TK4), CancellationToken, TV> func)
        {
            return (k1, k2, k3, k4, t) => func((k1, k2, k3, k4), t);
        }
        
        public static Func<TK, CancellationToken, TV> AppearCancellable<TK, TV>(
            this Func<TK, TV> func)
        {
            return (k, t) => func(k);
        }
        
        public static Func<TK1, TK2, CancellationToken, TV> AppearCancellable<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return (k1, k2, t) => func(k1, k2);
        }
        
        public static Func<TK1, TK2, TK3, CancellationToken, TV> AppearCancellable<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return (k1, k2, k3, t) => func(k1, k2, k3);
        }
        
        public static Func<TK1, TK2, TK3, TK4, CancellationToken, TV> AppearCancellable<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return (k1, k2, k3, k4, t) => func(k1, k2, k3, k4);
        }
        
        public static Func<TK, TV> MakeNonCancellable<TK, TV>(
            this Func<TK, CancellationToken, TV> func)
        {
            return k => func(k, CancellationToken.None);
        }
        
        public static Func<TK1, TK2, TV> MakeNonCancellable<TK1, TK2, TV>(
            this Func<TK1, TK2, CancellationToken, TV> func)
        {
            return (k1, k2) => func(k1, k2, CancellationToken.None);
        }
        
        public static Func<TK1, TK2, TK3, TV> MakeNonCancellable<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, CancellationToken, TV> func)
        {
            return (k1, k2, k3) => func(k1, k2, k3, CancellationToken.None);
        }
        
        public static Func<TK1, TK2, TK3, TK4, TV> MakeNonCancellable<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, CancellationToken, TV> func)
        {
            return (k1, k2, k3, k4) => func(k1, k2, k3, k4, CancellationToken.None);
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