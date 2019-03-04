using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal
{
    internal static class TupleKeyHelper
    {
        public static KeyComparer<(TK1, TK2)> BuildKeyComparer<TK1, TK2>(
            EqualityComparers keyComparers,
            int[] parametersToExcludeFromKey)
        {
            var key1Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? KeyComparerResolver.GetInner<TK1>(keyComparers)
                : new AlwaysEqualComparer<TK1>();

            var key2Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? KeyComparerResolver.GetInner<TK2>(keyComparers)
                : new AlwaysEqualComparer<TK2>();

            var combinedComparer = new ValueTupleComparer<TK1, TK2>(key1Comparer, key2Comparer);

            return new KeyComparer<(TK1, TK2)>(combinedComparer);
        }

        public static Func<(TK1, TK2), string> BuildKeySerializer<TK1, TK2>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var serializers = new List<Func<(TK1, TK2), string>>();

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0))
            {
                var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
                serializers.Add(x => key1Serializer(x.Item1));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1))
            {
                var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);
                serializers.Add(x => key2Serializer(x.Item2));
            }

            return Serialize;

            string Serialize((TK1, TK2) key)
            {
                return String.Join(keyParamSeparator, serializers.Select(s => s(key)));
            }
        }

        public static Func<string, (TK1, TK2)> BuildKeyDeserializer<TK1, TK2>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var key1Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? GetKeyDeserializerSingle<TK1>(keySerializers)
                : null;

            var key2Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? GetKeyDeserializerSingle<TK2>(keySerializers)
                : null;

            return Deserialize;

            (TK1, TK2) Deserialize(string str)
            {
                TK1 value1;
                TK2 value2;

                var separator = new StringSeparator(str, keyParamSeparator);

                if (key1Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value1 = key1Deserializer(value);
                }
                else
                {
                    value1 = default;
                }

                if (key2Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value2 = key2Deserializer(value);
                }
                else
                {
                    value2 = default;
                }

                return (value1, value2);
            }
        }

        public static KeyComparer<(TK1, TK2, TK3)> BuildKeyComparer<TK1, TK2, TK3>(
            EqualityComparers keyComparers,
            int[] parametersToExcludeFromKey)
        {
            var key1Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? KeyComparerResolver.GetInner<TK1>(keyComparers)
                : new AlwaysEqualComparer<TK1>();

            var key2Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? KeyComparerResolver.GetInner<TK2>(keyComparers)
                : new AlwaysEqualComparer<TK2>();

            var key3Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2)
                ? KeyComparerResolver.GetInner<TK3>(keyComparers)
                : new AlwaysEqualComparer<TK3>();

            var combinedComparer = new ValueTupleComparer<TK1, TK2, TK3>(key1Comparer, key2Comparer, key3Comparer);

            return new KeyComparer<(TK1, TK2, TK3)>(combinedComparer);
        }

        public static Func<(TK1, TK2, TK3), string> BuildKeySerializer<TK1, TK2, TK3>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var serializers = new List<Func<(TK1, TK2, TK3), string>>();

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0))
            {
                var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
                serializers.Add(x => key1Serializer(x.Item1));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1))
            {
                var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);
                serializers.Add(x => key2Serializer(x.Item2));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2))
            {
                var key3Serializer = GetKeySerializerSingle<TK3>(keySerializers);
                serializers.Add(x => key3Serializer(x.Item3));
            }

            return Serialize;

            string Serialize((TK1, TK2, TK3) key)
            {
                return String.Join(keyParamSeparator, serializers.Select(s => s(key)));
            }
        }

        public static Func<string, (TK1, TK2, TK3)> BuildKeyDeserializer<TK1, TK2, TK3>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var key1Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? GetKeyDeserializerSingle<TK1>(keySerializers)
                : null;

            var key2Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? GetKeyDeserializerSingle<TK2>(keySerializers)
                : null;

            var key3Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2)
                ? GetKeyDeserializerSingle<TK3>(keySerializers)
                : null;

            return Deserialize;

            (TK1, TK2, TK3) Deserialize(string str)
            {
                TK1 value1;
                TK2 value2;
                TK3 value3;

                var separator = new StringSeparator(str, keyParamSeparator);

                if (key1Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value1 = key1Deserializer(value);
                }
                else
                {
                    value1 = default;
                }

                if (key2Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value2 = key2Deserializer(value);
                }
                else
                {
                    value2 = default;
                }

                if (key3Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value3 = key3Deserializer(value);
                }
                else
                {
                    value3 = default;
                }

                return (value1, value2, value3);
            }
        }

        public static KeyComparer<(TK1, TK2, TK3, TK4)> BuildKeyComparer<TK1, TK2, TK3, TK4>(
            EqualityComparers keyComparers,
            int[] parametersToExcludeFromKey)
        {
            var key1Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? KeyComparerResolver.GetInner<TK1>(keyComparers)
                : new AlwaysEqualComparer<TK1>();

            var key2Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? KeyComparerResolver.GetInner<TK2>(keyComparers)
                : new AlwaysEqualComparer<TK2>();

            var key3Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2)
                ? KeyComparerResolver.GetInner<TK3>(keyComparers)
                : new AlwaysEqualComparer<TK3>();

            var key4Comparer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(3)
                ? KeyComparerResolver.GetInner<TK4>(keyComparers)
                : new AlwaysEqualComparer<TK4>();

            var combinedComparer = new ValueTupleComparer<TK1, TK2, TK3, TK4>(
                key1Comparer,
                key2Comparer,
                key3Comparer,
                key4Comparer);

            return new KeyComparer<(TK1, TK2, TK3, TK4)>(combinedComparer);
        }

        public static Func<(TK1, TK2, TK3, TK4), string> BuildKeySerializer<TK1, TK2, TK3, TK4>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var serializers = new List<Func<(TK1, TK2, TK3, TK4), string>>();

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0))
            {
                var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
                serializers.Add(x => key1Serializer(x.Item1));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1))
            {
                var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);
                serializers.Add(x => key2Serializer(x.Item2));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2))
            {
                var key3Serializer = GetKeySerializerSingle<TK3>(keySerializers);
                serializers.Add(x => key3Serializer(x.Item3));
            }

            if (parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(3))
            {
                var key4Serializer = GetKeySerializerSingle<TK4>(keySerializers);
                serializers.Add(x => key4Serializer(x.Item4));
            }

            return Serialize;

            string Serialize((TK1, TK2, TK3, TK4) key)
            {
                return String.Join(keyParamSeparator, serializers.Select(s => s(key)));
            }
        }

        public static Func<string, (TK1, TK2, TK3, TK4)> BuildKeyDeserializer<TK1, TK2, TK3, TK4>(
            KeySerializers keySerializers,
            string keyParamSeparator,
            int[] parametersToExcludeFromKey)
        {
            var key1Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(0)
                ? GetKeyDeserializerSingle<TK1>(keySerializers)
                : null;

            var key2Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(1)
                ? GetKeyDeserializerSingle<TK2>(keySerializers)
                : null;

            var key3Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(2)
                ? GetKeyDeserializerSingle<TK3>(keySerializers)
                : null;

            var key4Deserializer = parametersToExcludeFromKey == null || !parametersToExcludeFromKey.Contains(3)
                ? GetKeyDeserializerSingle<TK4>(keySerializers)
                : null;

            return Deserialize;

            (TK1, TK2, TK3, TK4) Deserialize(string str)
            {
                TK1 value1;
                TK2 value2;
                TK3 value3;
                TK4 value4;

                var separator = new StringSeparator(str, keyParamSeparator);

                if (key1Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value1 = key1Deserializer(value);
                }
                else
                {
                    value1 = default;
                }

                if (key2Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value2 = key2Deserializer(value);
                }
                else
                {
                    value2 = default;
                }

                if (key3Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value3 = key3Deserializer(value);
                }
                else
                {
                    value3 = default;
                }

                if (key4Deserializer != null)
                {
                    if (!separator.TryGetNext(out var value))
                        throw new Exception("");

                    value4 = key4Deserializer(value);
                }
                else
                {
                    value4 = default;
                }

                return (value1, value2, value3, value4);
            }
        }

        private static Func<T, string> GetKeySerializerSingle<T>(KeySerializers keySerializers)
        {
            if (keySerializers.TryGetSerializer<T>(out var serializer))
                return serializer;

            if (serializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetSerializer(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(T).FullName}'"));
        }

        private static Func<string, T> GetKeyDeserializerSingle<T>(KeySerializers keySerializers)
        {
            if (keySerializers.TryGetDeserializer<T>(out var deserializer))
                return deserializer;

            if (deserializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetDeserializer(out deserializer);

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(T).FullName}'"));
        }
    }
}
