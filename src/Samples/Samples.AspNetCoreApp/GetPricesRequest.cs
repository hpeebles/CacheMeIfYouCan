using System.Collections.Generic;

namespace Samples.AspNetCoreApp
{
    public class GetPricesRequest
    {
        public int AffiliateId;
        public IList<int> ItemIds;
    }
}