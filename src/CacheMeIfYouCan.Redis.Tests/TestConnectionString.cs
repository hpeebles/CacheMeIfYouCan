using System;

namespace CacheMeIfYouCan.Redis.Tests
{
    public static class TestConnectionString
    {
        public static readonly string Value = $"redis-test:{Port},password={Password}";

        private static string Password => Environment.GetEnvironmentVariable("RedisTestPassword");
        private static int Port => Int32.Parse(Environment.GetEnvironmentVariable("RedisTestPort"));
    }
}