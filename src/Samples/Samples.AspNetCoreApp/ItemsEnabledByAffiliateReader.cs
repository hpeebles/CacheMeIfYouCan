using System;
using System.Linq;
using System.Threading.Tasks;

namespace Samples.AspNetCoreApp
{
    public interface IItemsEnabledByAffiliateReader
    {
        Task<ILookup<int, int>> Get();
    }
    
    public class ItemsEnabledByAffiliateReader : IItemsEnabledByAffiliateReader
    {
        public async Task<ILookup<int, int>> Get()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            return new (int affiliateId, int itemId)[]
                {
                    (1, 1),
                    (1, 2),
                    (2, 2),
                    (2, 3)
                }
                .ToLookup(t => t.affiliateId, t => t.itemId);
        }
    }
}