using Ordering.Application.Orders.GetOrders;

namespace Ordering.API.Order.GetOrders
{
    public static class GetOrdersEndpoint
    {

        public static void GetOrders(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetOrdersQuery(request));

                var response = result.ToGetOrdersResponse();

                return Results.Ok(response);
            })
            .WithName("GetOrders")
            .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Orders")
            .WithDescription("Get Orders");
        }
    }
}
