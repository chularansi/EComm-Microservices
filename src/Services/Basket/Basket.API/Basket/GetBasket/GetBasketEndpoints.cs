namespace Basket.API.Basket.GetBasket
{
    public static class GetBasketEndpoints
    {
        public static void GetBasket(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{userName}", async (string userName, ISender sender) =>
            {
                var result = await sender.Send(new GetBasketQuery(userName));

                var respose = result.ToGetBasketResponse();

                return Results.Ok(respose);
            })
            .WithName("GetProductById")
            .Produces<GetBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Id")
            .WithDescription("Get Product By Id");
        }
    }
}
