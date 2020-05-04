using System;
using System.Diagnostics;
using System.Threading;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class StopwatchStructTests
    {
        [Fact]
        public void MeasuresTimeAccurately()
        {
            var timer1 = StopwatchStruct.StartNew();
            var timer2 = Stopwatch.StartNew();
            
            Thread.Sleep(TimeSpan.FromSeconds(1));

            timer1.Elapsed.Should().BeCloseTo(timer2.Elapsed);
        }
    }
}