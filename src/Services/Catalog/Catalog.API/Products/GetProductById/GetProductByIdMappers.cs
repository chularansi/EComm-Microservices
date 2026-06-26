using Catalog.API.Products.DeleteProduct;

namespace Catalog.API.Products.GetProductById
{
    public static class GetProductByIdMappers
    {
        public static GetProductByIdResponse ToProductByIdResponse(this GetProductByIdResult result)
        {
            return new GetProductByIdResponse(result.Product);
        }
    }
}
