namespace Basket.API.Basket.GetBasket
{
    public static class GetBasketMappers
    {
        public static GetBasketResponse ToGetBasketResponse(this GetBasketResult result)
        {
            return new GetBasketResponse(result.Cart);
        }
    }
}
