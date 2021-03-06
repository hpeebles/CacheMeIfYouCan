﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    // Publicly exposed functionality is covered by the tests in LocalCacheTests
    // This set of tests is to ensure the inner workings are operating correctly
    public class DictionaryCacheTests
    {
        [Fact]
        public void ValueAndExpiry_ReferencesAreRecycledWhenKeysExpire()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(100));
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(100);
        }
        
        [Fact]
        public void ValueAndExpiry_ReferencesAreRecycledWhenKeysAreUpdated()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            cache.Set(1, 1, TimeSpan.FromSeconds(1));
            cache.Set(1, 1, TimeSpan.FromSeconds(1));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().HaveCount(1);
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(1);
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(2);
        }

        [Fact]
        public void ValueAndExpiry_TakeFromRecycleQueueBeforeCreatingNew()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(100);
            
            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromSeconds(1));

            debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().HaveCount(100);
            debugInfo.ValueAndExpiryPool.PeekAll().Should().BeEmpty();
        }
        
        [Fact]
        public void ValueAndExpiry_CountOfReferencesToRecycleIsLimited()
        {
            const int maxItemsInRecycleQueue = 1000;
            
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < maxItemsInRecycleQueue * 2; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            var debugInfo = cache.GetDebugInfo();

            debugInfo.Values.Should().BeEmpty();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(maxItemsInRecycleQueue);
        }

        [Fact]
        public void WhenKeyUpdated_KeyExpiryTimeStillProcessedAsNormal()
        {
            var cache = new DictionaryCache<int, int>(EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(100));

            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(10));
            
            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromMilliseconds(500));
            
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            
            for (var i = 0; i < 100; i++)
                cache.TryGet(i, out _).Should().BeTrue();
            
            var debugInfo = cache.GetDebugInfo();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(1);
            
            Thread.Sleep(TimeSpan.FromSeconds(3));

            debugInfo = cache.GetDebugInfo();
            debugInfo.ValueAndExpiryPool.PeekAll().Should().HaveCount(101);
            
            for (var i = 0; i < 100; i++)
                cache.TryGet(i, out _).Should().BeFalse();
        }
    }
}