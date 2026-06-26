namespace Catalog.API.Products.GetProductById
{
    public static class GetProductByIdEndpoint
    {
        public static void GetProductById(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id));

                var response = result.ToProductByIdResponse();

                return Results.Ok(response);
            })
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Id")
            .WithDescription("Get Product By Id");
        }
    }
}
