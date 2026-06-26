namespace Basket.API.Basket.GetBasket
{
    public record GetBasketRequest(string UserName); 
    public record GetBasketResponse(ShoppingCart Cart);

    public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
    public record GetBasketResult(ShoppingCart Cart);
}
