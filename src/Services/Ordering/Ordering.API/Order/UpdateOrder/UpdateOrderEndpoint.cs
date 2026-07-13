namespace Ordering.API.Order.UpdateOrder
{
    public static class UpdateOrderEndpoint
    {
        public static void UpdateOrder(this IEndpointRouteBuilder app)
        {
            app.MapPut("/", async (UpdateOrderRequest request, ISender sender) =>
            {
                var command = request.ToUpdateOrderCommand();

                var result = await sender.Send(command);

                var response = result.ToUpdateOrderResponse();

                return Results.Ok(response);
            })
            .WithName("UpdateOrder")
            .Produces<UpdateOrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Order")
            .WithDescription("Update Order");
        }
    }
}
