namespace Catalog.API.Products.DeleteProduct
{
    public static class DeleteProductEndpoint
    {
        public static void DeleteProduct(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id));

                var response = result.ToDeleteProductResponse();

                return Results.Ok(response);
            })
            .WithName("DeleteProduct")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Product")
            .WithDescription("Delete Product");
        }
    }
}
