namespace Samples.AspNetCoreApp
{
    public struct ItemPrice
    {
        public ItemPrice(int itemId, decimal price)
        {
            ItemId = itemId;
            Price = price;
        }
        
        public int ItemId { get; }
        public decimal Price { get; }
    }
}