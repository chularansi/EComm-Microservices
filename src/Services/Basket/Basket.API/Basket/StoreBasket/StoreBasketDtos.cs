namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketRequest(ShoppingCart Cart);

    // When creating StoreBasketCommand, we have to pass the ShoppingCart parametername as Cart
    // because the request is json serialized with the property name Cart,
    // and the command handler will use that property name to bind the value to the command parameter.
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);
    public record StoreBasketResponse(string UserName);

}
