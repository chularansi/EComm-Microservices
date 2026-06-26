namespace Catalog.API.Products.GetProducts
{
    public static class GetProductsEndpoint
    {
        public static void GetProducts(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", async ([AsParameters] GetProductsRequest request, ISender sender) =>
            {
                var query = request.ToGetProductsQuery();

                var result = await sender.Send(query);

                var response = result.ToGetProductsResponse();

                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Products")
            .WithDescription("Get Products");
        }
    }
}
