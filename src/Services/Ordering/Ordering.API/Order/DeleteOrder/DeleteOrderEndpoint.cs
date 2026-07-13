using Ordering.Application.Orders.DeleteOrder;

namespace Ordering.API.Order.DeleteOrder
{
    public static class DeleteOrderEndpoint
    {
        public static void DeleteOrder(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:guid}", async (Guid Id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteOrderCommand(Id));

                var response = result.ToDeleteOrderResponse();

                return Results.Ok(response);
            })
            .WithName("DeleteOrder")
            .Produces<DeleteOrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Order")
            .WithDescription("Delete Order");
        }
    }
}
