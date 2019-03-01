using System;
using System.Threading;

namespace CacheMeIfYouCan.Configuration
{
    internal static class MultiParamEnumerableKeyFunctionConverters
    {
        public static Func<(TKOuter1, TKOuter2), TKInnerEnumerable, CancellationToken, TRes> ConvertFunc<TKOuter1, TKOuter2, TKInnerEnumerable, TRes>(
            this Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, TRes> inputFunc)
        {
            return (ko, ki, t) => inputFunc(ko.Item1, ko.Item2, ki, t);
        }
        
        public static Func<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, CancellationToken, TRes> ConvertFunc<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes>(
            this Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, TRes> inputFunc)
        {
            return (ko, ki, t) => inputFunc(ko.Item1, ko.Item2, ko.Item3, ki, t);
        }
        
        public static Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, TRes> ConvertFunc<TKOuter1, TKOuter2, TKInnerEnumerable, TRes>(
            this Func<(TKOuter1, TKOuter2), TKInnerEnumerable, CancellationToken, TRes> inputFunc)
        {
            return (ko1, ko2, ki, t) => inputFunc((ko1, ko2), ki, t);
        }
        
        public static Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, TRes> ConvertFunc<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes>(
            this Func<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, CancellationToken, TRes> inputFunc)
        {
            return (ko1, ko2, ko3, ki, t) => inputFunc((ko1, ko2, ko3), ki, t);
        }
    }
}