using System;

namespace CacheMeIfYouCan.Configuration
{
    internal static class MultiParamEnumerableKeyFunctionConverters
    {
        public static Func<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes> ConvertFunc<TKOuter1, TKOuter2, TKInnerEnumerable, TRes>(
            this Func<TKOuter1, TKOuter2, TKInnerEnumerable, TRes> inputFunc)
        {
            return (ko, ki) => inputFunc(ko.Item1, ko.Item2, ki);
        }
        
        public static Func<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes> ConvertFunc<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes>(
            this Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes> inputFunc)
        {
            return (ko, ki) => inputFunc(ko.Item1, ko.Item2, ko.Item3, ki);
        }
        
        public static Func<TKOuter1, TKOuter2, TKInnerEnumerable, TRes> ConvertFunc<TKOuter1, TKOuter2, TKInnerEnumerable, TRes>(
            this Func<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes> inputFunc)
        {
            return (ko1, ko2, ki) => inputFunc((ko1, ko2), ki);
        }
        
        public static Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes> ConvertFunc<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes>(
            this Func<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes> inputFunc)
        {
            return (ko1, ko2, ko3, ki) => inputFunc((ko1, ko2, ko3), ki);
        }
    }
}