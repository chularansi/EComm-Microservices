using Catalog.API.Products.CreateProduct;

namespace Catalog.API.Products.DeleteProduct
{
    public static class DeleteProductMappers
    {
        public static DeleteProductResponse ToDeleteProductResponse(this DeleteProductResult result)
        {
            return new DeleteProductResponse(result.IsSuccess);
        }
    }
}
