namespace Basket.API.Basket.StoreBasket
{
    public static class StoreBasketMappers
    {
        public static StoreBasketCommand ToStoreBasketCommand(this StoreBasketRequest request)
        {
            return new StoreBasketCommand(request.Cart);
        }

        public static StoreBasketResponse ToStoreBasketResponse(this StoreBasketResult result)
        {
            return new StoreBasketResponse(result.UserName);
        }
    }
}
