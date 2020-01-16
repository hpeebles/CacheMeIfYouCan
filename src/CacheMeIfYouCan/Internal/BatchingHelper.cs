using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class BatchingHelper
    {
        public static IReadOnlyCollection<IReadOnlyCollection<T>> Batch<T>(
            IReadOnlyCollection<T> values,
            int maxBatchSize,
            BatchBehaviour batchBehaviour)
        {
            var batchCount = ((values.Count - 1) / maxBatchSize) + 1;
            var batches = new T[batchCount][];

            if (batchBehaviour == BatchBehaviour.FillBatchesEvenly)
            {
                var averageBatchSize = (double)values.Count / batchCount;
                var totalAllocated = 0;
                for (var index = 0; index < batchCount - 1; index++)
                {
                    var nextBatchSize = (int)(averageBatchSize * (index + 1)) - totalAllocated;
                    batches[index] = new T[nextBatchSize];
                    totalAllocated += nextBatchSize;
                }

                var finalBatchSize = values.Count - totalAllocated;
                batches[batchCount - 1] = new T[finalBatchSize];
            }
            else
            {
                for (var index = 0; index < batchCount - 1; index++)
                    batches[index] = new T[maxBatchSize];

                var finalBatchSize = values.Count - ((batchCount - 1) * maxBatchSize);
                batches[batchCount - 1] = new T[finalBatchSize];
            }
            
            var currentBatchIndex = 0;
            var currentBatch = batches[0];
            var indexWithinBatch = 0;
            foreach (var value in values)
            {
                if (indexWithinBatch == currentBatch.Length)
                {
                    currentBatch = batches[++currentBatchIndex];
                    indexWithinBatch = 0;
                }
                
                currentBatch[indexWithinBatch++] = value;
            }

            return batches;
        }
    }
}