using System;
using System.Collections.Generic;
using System.Threading;
using CacheMeIfYouCan.LocalCaches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    // Publicly exposed functionality is covered by the tests in LocalCacheTests
    // This set of tests is to ensure the inner workings are operating correctly
    public class DictionaryCacheTests
    {
        [Fact]
        public void KeyAndExpiry_ValueAndExpiry_ReferencesAreRecycledWhenKeysExpire()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(1.5));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.KeysToRecycle.Should().HaveCount(100);
            debugInfo.ValuesToRecycle.Should().HaveCount(100);
        }
        
        [Fact]
        public void KeyAndExpiry_ValueAndExpiry_ReferencesAreRecycledWhenKeysAreUpdated()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            cache.Set(1, 1, TimeSpan.FromSeconds(1));
            cache.Set(1, 1, TimeSpan.FromSeconds(1));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().HaveCount(1);
            debugInfo.KeysToRecycle.Should().BeEmpty();
            debugInfo.ValuesToRecycle.Should().HaveCount(1);
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.KeysToRecycle.Should().HaveCount(2);
            debugInfo.ValuesToRecycle.Should().HaveCount(2);
        }

        [Fact]
        public void KeyAndExpiry_ValueAndExpiry_TakeFromRecycleQueueBeforeCreatingNew()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(1.5));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.KeysToRecycle.Should().HaveCount(100);
            debugInfo.ValuesToRecycle.Should().HaveCount(100);
            
            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromSeconds(1));

            debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().HaveCount(100);
            debugInfo.KeysToRecycle.Should().BeEmpty();
            debugInfo.ValuesToRecycle.Should().BeEmpty();
        }
        
        [Fact]
        public void KeyAndExpiry_ValueAndExpiry_CountOfReferencesToRecycleIsLimited()
        {
            const int maxItemsInRecycleQueues = 1000;
            
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < maxItemsInRecycleQueues * 2; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(1.5));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.KeysToRecycle.Should().HaveCount(maxItemsInRecycleQueues);
            debugInfo.ValuesToRecycle.Should().HaveCount(maxItemsInRecycleQueues);
        }
    }
}