
namespace Catalog.API.Products.UpdateProduct
{
    public static class UpdateProductEndpoint
    {
        public static void UpdateProduct(this IEndpointRouteBuilder app)
        {
            app.MapPut("/", 
                async (UpdateProductRequest request, ISender sender) =>
                {
                    var command = request.ToUpdateProductCommand();

                    var result = await sender.Send(command);

                    var response = result.ToUpdateProductResponse();

                    return Results.Ok(response);
                })
                .WithName("UpdateProduct")
                .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Update Product")
                .WithDescription("Update Product");
        }
    }
}
