using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan;
using Microsoft.AspNetCore.Mvc;

namespace Samples.AspNetCoreApp
{
    public class PricesController : Controller
    {
        private readonly ICachedObject<ISet<int>> _enabledAffiliatesCache;
        private readonly IItemPriceReader _priceReader;

        public PricesController(
            ICachedObject<ISet<int>> enabledAffiliatesCache,
            IItemPriceReader priceReader)
        {
            _enabledAffiliatesCache = enabledAffiliatesCache;
            _priceReader = priceReader;
        }

        [HttpPost, Route("api/getprices")]
        public async Task<ActionResult<Dictionary<int, decimal>>> GetPrices([FromBody]GetPricesRequest request)
        {
            var enabledAffiliates = _enabledAffiliatesCache.Value;

            if (!enabledAffiliates.Contains(request.AffiliateId))
                return BadRequest($"Affiliate {request.AffiliateId} is not enabled");

            var itemPrices = await _priceReader.GetPrices(request.ItemIds);

            return Ok(itemPrices);
        }
        
        [HttpPost, Route("api/getprice")]
        public async Task<ActionResult<decimal>> GetPrice([FromBody]GetPriceRequest request)
        {
            var enabledAffiliates = _enabledAffiliatesCache.Value;

            if (!enabledAffiliates.Contains(request.AffiliateId))
                return BadRequest($"Affiliate {request.AffiliateId} is not enabled");

            var itemPrice = await _priceReader.GetPrice(request.ItemId);

            return Ok(itemPrice);
        }
    }
}