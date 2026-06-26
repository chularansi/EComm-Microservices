
namespace Catalog.API.Products.GetProductByCategory
{
    public static class GetProductByCategoryEndpoint
    {
        public static void GetProductByCategory(this IEndpointRouteBuilder app)
        {
            app.MapGet("/category/{category}", 
                async (string category, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category));
            
                var response = result.ToProductByCategoryResponse();
            
                return Results.Ok(response);
            })
            .WithName("GetProductByCategory")
            .Produces<GetProductByCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Category")
            .WithDescription("Get Product By Category");
        }
    }
}
