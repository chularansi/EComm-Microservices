namespace Basket.API.Basket.CheckoutBasket
{
    public static class CheckoutBasketEndpoints
    {
        public static void CheckoutBasket(this IEndpointRouteBuilder app)
        {
            app.MapPost("/checkout", async (CheckoutBasketRequest request, ISender sender) =>
            {
                var command = request.ToCheckoutBasketCommand();

                var result = await sender.Send(command);

                var response = result.ToCheckoutBasketResponse();

                return Results.Ok(response);
            })
            .WithName("CheckoutBasket")
            .Produces<CheckoutBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Checkout Basket")
            .WithDescription("Checkout Basket");
        }
    }
}
