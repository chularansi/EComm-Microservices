namespace Catalog.API.Products.CreateProduct
{
    public static class CreateProductEndpoint
    {
        public static void CreateProduct(this IEndpointRouteBuilder app)
        {
            app.MapPost("/",
                static async (CreateProductRequest request, ISender sender) =>
            {
                var command = request.ToCreateProductCommand();

                var result = await sender.Send(command);

                var response = result.ToCreateProductResponse();

                return Results.Created($"/products/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");
        }
    }
}
