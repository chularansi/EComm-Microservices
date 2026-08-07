namespace Catalog.API.Products.UpdateProduct
{
    public record UpdateProductRequest(Guid Id, string Name, List<string> Category, string Description, string Color, string ImageFile, decimal Price);
    public record UpdateProductResponse(bool IsSuccess);

    public record UpdateProductCommand(Guid Id, string Name, List<string> Category, string Description, string Color, string ImageFile, decimal Price)
        : ICommand<UpdateProductResult>;
    public record UpdateProductResult(bool IsSuccess);
}
