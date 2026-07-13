using Ordering.Application.Orders.GetOrdersByName;

namespace Ordering.API.Order.GetOrdersByName
{
    public static class GetOrdersByNameEndpoint
    {
        public static void GetOrdersByName(this IEndpointRouteBuilder app) 
        {
            app.MapGet("/{orderName}", async(string orderName, ISender sender) =>
            {
                var result = await sender.Send(new GetOrdersByNameQuery(orderName));

                var response = result.ToGetOrdersByNameResponse();

                return Results.Ok(response);
            })
            .WithName("GetOrdersByName")
            .Produces<GetOrdersByNameResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Orders By Name")
            .WithDescription("Get Orders By Name");
        }
    }
}
