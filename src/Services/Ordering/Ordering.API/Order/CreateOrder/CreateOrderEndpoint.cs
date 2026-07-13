namespace Ordering.API.Order.CreateOrder
{
    public static class CreateOrderEndpoint
    {
        public static void CreateOrder(this IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (CreateOrderRequest request, ISender sender) =>
            {
                var command = request.ToCreateOrderCommand();

                var result = await sender.Send(command);

                var response = result.ToCreateOrderResponse();

                return Results.Created($"/orders/{response.Id}", response);
            })
            .WithName("CreateOrder")
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Order")
            .WithDescription("Create Order");
        }
    }
}
