using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samples.AspNetCoreApp
{
    public interface IItemPriceReader
    {
        Task<decimal> GetPrice(int itemId);
        
        Task<Dictionary<int, decimal>> GetPrices(IList<int> itemIds);
    }
    
    public class ItemPriceReader : IItemPriceReader
    {
        public async Task<decimal> GetPrice(int itemId)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            return itemId * 10m;
        }
        
        public async Task<Dictionary<int, decimal>> GetPrices(IList<int> itemIds)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            return itemIds.ToDictionary(id => id, id => id * 10m);
        }
    }
}