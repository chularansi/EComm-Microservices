namespace Catalog.API.Products.GetProductById
{
    public record GetProductByIdResponse(Product Product);

    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;
    public record GetProductByIdResult(Product Product);
}
