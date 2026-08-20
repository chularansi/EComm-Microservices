namespace Basket.API.Basket.StoreBasket
{
    public static class StoreBasketEndpoint
    {
        public static void StoreBasket(this IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (StoreBasketRequest request, ISender sender) =>
            {
                var command = request.ToStoreBasketCommand();

                var result = await sender.Send(command);

                var response = result.ToStoreBasketResponse();

                return Results.Created($"/basket/{response.UserName}", response);
            })
            .WithName("CreateProduct")
            .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");
        }
    }
}
