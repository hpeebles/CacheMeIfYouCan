using System;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    internal static class TupleHelper
    {
        public static Func<(TParam1, TParam2), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, TValue> func)
        {
            return t => func(t.Item1, t.Item2);
        }

        public static Func<(TParam1, TParam2, TParam3), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3);
        }
        
        public static Func<(TParam1, TParam2, TParam3, TParam4), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3, t.Item4);
        }
        
        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5);
        }
        
        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6);
        }
        
        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7);
        }
        
        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> func)
        {
            return t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8);
        }
        
        public static Func<(TParam1, TParam2), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3, TParam4), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, t.Item4, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, cancellationToken);
        }

        public static Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), CancellationToken, TValue> ConvertFuncToTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> func)
        {
            return (t, cancellationToken) => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TValue>(
            Func<(TParam1, TParam2), TValue> func)
        {
            return (p1, p2) => func((p1, p2));
        }

        public static Func<TParam1, TParam2, TParam3, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TValue>(
            Func<(TParam1, TParam2, TParam3), TValue> func)
        {
            return (p1, p2, p3) => func((p1, p2, p3));
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4), TValue> func)
        {
            return (p1, p2, p3, p4) => func((p1, p2, p3, p4));
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5), TValue> func)
        {
            return (p1, p2, p3, p4, p5) => func((p1, p2, p3, p4, p5));
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6) => func((p1, p2, p3, p4, p5, p6));
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6, p7) => func((p1, p2, p3, p4, p5, p6, p7));
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6, p7, p8) => func((p1, p2, p3, p4, p5, p6, p7, p8));
        }
        
        public static Func<TParam1, TParam2, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TValue>(
            Func<(TParam1, TParam2), CancellationToken, TValue> func)
        {
            return (p1, p2, cancellationToken) => func((p1, p2), cancellationToken);
        }

        public static Func<TParam1, TParam2, TParam3, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TValue>(
            Func<(TParam1, TParam2, TParam3), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, cancellationToken) => func((p1, p2, p3), cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, p4, cancellationToken) => func((p1, p2, p3, p4), cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, p4, p5, cancellationToken) => func((p1, p2, p3, p4, p5), cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6, cancellationToken) => func((p1, p2, p3, p4, p5, p6), cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => func((p1, p2, p3, p4, p5, p6, p7), cancellationToken);
        }
        
        public static Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> ConvertFuncFromTupleInput<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), CancellationToken, TValue> func)
        {
            return (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => func((p1, p2, p3, p4, p5, p6, p7, p8), cancellationToken);
        }
    }
}