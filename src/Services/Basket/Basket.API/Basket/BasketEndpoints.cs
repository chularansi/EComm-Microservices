using Basket.API.Basket.DeleteBasket;
using Basket.API.Basket.GetBasket;
using Basket.API.Basket.StoreBasket;

namespace Basket.API.Basket
{
    public static class BasketEndpoints
    {
        public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/basket");

            group.StoreBasket();
            group.DeleteBasket();
            group.GetBasket();
        }
    }
}
