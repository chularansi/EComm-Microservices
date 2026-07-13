using Ordering.Application.Orders.GetOrdersByCustomer;

namespace Ordering.API.Order.GetOrdersByCustomer
{
    public static class GetOrdersByCustomerEndpoint
    {
        public static void GetOrdersByCustomer(this IEndpointRouteBuilder app)
        {
            app.MapGet("/customer/{customerId}", async (Guid customerId, ISender sender) =>
            {
                var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));

                var response = result.ToGetOrdersByCustomerResponse();

                return Results.Ok(response);
            })
            .WithName("GetOrdersByCustomer")
            .Produces<GetOrdersByCustomerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Orders By Customer")
            .WithDescription("Get Orders By Customer");
        }
    }
}
