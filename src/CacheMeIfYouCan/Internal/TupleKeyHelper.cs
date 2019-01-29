using System;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal
{
    internal static class TupleKeyHelper
    {
        public static KeyComparer<(TK1, TK2)> BuildKeyComparer<TK1, TK2>(EqualityComparers keyComparers)
        {
            var key1Comparer = KeyComparerResolver.GetInner<TK1>(keyComparers);
            var key2Comparer = KeyComparerResolver.GetInner<TK2>(keyComparers);

            var combinedComparer = new ValueTupleComparer<TK1, TK2>(key1Comparer, key2Comparer);

            return new KeyComparer<(TK1, TK2)>(combinedComparer);
        }

        public static Func<(TK1, TK2), string> BuildKeySerializer<TK1, TK2>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
            var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);

            return x =>
                key1Serializer(x.Item1) +
                keyParamSeparator +
                key2Serializer(x.Item2);
        }

        public static Func<string, (TK1, TK2)> BuildKeyDeserializer<TK1, TK2>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Deserializer = GetKeyDeserializerSingle<TK1>(keySerializers);
            var key2Deserializer = GetKeyDeserializerSingle<TK2>(keySerializers);

            return Deserialize;

            (TK1, TK2) Deserialize(string str)
            {
                var index = str.IndexOf(keyParamSeparator, StringComparison.Ordinal);

                var k1 = str.Substring(0, index - 1);
                var k2 = str.Substring(index + keyParamSeparator.Length);

                return (key1Deserializer(k1), key2Deserializer(k2));
            }
        }

        public static KeyComparer<(TK1, TK2, TK3)> BuildKeyComparer<TK1, TK2, TK3>(EqualityComparers keyComparers)
        {
            var key1Comparer = KeyComparerResolver.GetInner<TK1>(keyComparers);
            var key2Comparer = KeyComparerResolver.GetInner<TK2>(keyComparers);
            var key3Comparer = KeyComparerResolver.GetInner<TK3>(keyComparers);

            var combinedComparer = new ValueTupleComparer<TK1, TK2, TK3>(key1Comparer, key2Comparer, key3Comparer);

            return new KeyComparer<(TK1, TK2, TK3)>(combinedComparer);
        }

        public static Func<(TK1, TK2, TK3), string> BuildKeySerializer<TK1, TK2, TK3>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
            var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);
            var key3Serializer = GetKeySerializerSingle<TK3>(keySerializers);

            return x =>
                key1Serializer(x.Item1) +
                keyParamSeparator +
                key2Serializer(x.Item2) +
                keyParamSeparator +
                key3Serializer;
        }

        public static Func<string, (TK1, TK2, TK3)> BuildKeyDeserializer<TK1, TK2, TK3>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Deserializer = GetKeyDeserializerSingle<TK1>(keySerializers);
            var key2Deserializer = GetKeyDeserializerSingle<TK2>(keySerializers);
            var key3Deserializer = GetKeyDeserializerSingle<TK3>(keySerializers);

            return Deserialize;

            (TK1, TK2, TK3) Deserialize(string str)
            {
                var parts = str.Split(new[] {keyParamSeparator}, 3, StringSplitOptions.None);

                return (
                    key1Deserializer(parts[0]),
                    key2Deserializer(parts[1]),
                    key3Deserializer(parts[2]));
            }
        }

        public static KeyComparer<(TK1, TK2, TK3, TK4)> BuildKeyComparer<TK1, TK2, TK3, TK4>(
            EqualityComparers keyComparers)
        {
            var key1Comparer = KeyComparerResolver.GetInner<TK1>(keyComparers);
            var key2Comparer = KeyComparerResolver.GetInner<TK2>(keyComparers);
            var key3Comparer = KeyComparerResolver.GetInner<TK3>(keyComparers);
            var key4Comparer = KeyComparerResolver.GetInner<TK4>(keyComparers);

            var combinedComparer = new ValueTupleComparer<TK1, TK2, TK3, TK4>(
                key1Comparer,
                key2Comparer,
                key3Comparer,
                key4Comparer);

            return new KeyComparer<(TK1, TK2, TK3, TK4)>(combinedComparer);
        }

        public static Func<(TK1, TK2, TK3, TK4), string> BuildKeySerializer<TK1, TK2, TK3, TK4>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Serializer = GetKeySerializerSingle<TK1>(keySerializers);
            var key2Serializer = GetKeySerializerSingle<TK2>(keySerializers);
            var key3Serializer = GetKeySerializerSingle<TK3>(keySerializers);
            var key4Serializer = GetKeySerializerSingle<TK3>(keySerializers);

            return x =>
                key1Serializer(x.Item1) +
                keyParamSeparator +
                key2Serializer(x.Item2) +
                keyParamSeparator +
                key3Serializer +
                keyParamSeparator +
                key4Serializer;
        }

        public static Func<string, (TK1, TK2, TK3, TK4)> BuildKeyDeserializer<TK1, TK2, TK3, TK4>(
            KeySerializers keySerializers,
            string keyParamSeparator)
        {
            var key1Deserializer = GetKeyDeserializerSingle<TK1>(keySerializers);
            var key2Deserializer = GetKeyDeserializerSingle<TK2>(keySerializers);
            var key3Deserializer = GetKeyDeserializerSingle<TK3>(keySerializers);
            var key4Deserializer = GetKeyDeserializerSingle<TK4>(keySerializers);

            return Deserialize;

            (TK1, TK2, TK3, TK4) Deserialize(string str)
            {
                var parts = str.Split(new[] {keyParamSeparator}, 4, StringSplitOptions.None);

                return (
                    key1Deserializer(parts[0]),
                    key2Deserializer(parts[1]),
                    key3Deserializer(parts[2]),
                    key4Deserializer(parts[3]));
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