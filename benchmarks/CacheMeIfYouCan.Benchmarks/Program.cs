﻿using System;
using System.Linq;

namespace CacheMeIfYouCan.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmarkId = GetBenchmarkId(args);

            switch (benchmarkId)
            {
                case 0:
                    CachedFunctionWithSingleKey.Run();
                    break;
                case 1:
                    CachedFunctionWithEnumerableKeys.Run();
                    break;
                case 2:
                    LocalCaches.Run();
                    break;
                default:
                    Console.WriteLine("No benchmarks found with Id - " + benchmarkId);
                    break;
            }
        }

        private static int GetBenchmarkId(string[] args)
        {
            if (args is null || !args.Any() || !Int32.TryParse(args[0], out var id))
                return 0;

            return id;
        }
    }
}