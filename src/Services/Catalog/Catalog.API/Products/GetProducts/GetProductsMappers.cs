using Catalog.API.Products.CreateProduct;
using Catalog.API.Products.GetProductById;

namespace Catalog.API.Products.GetProducts
{
    public static class GetProductsMappers
    {
        public static GetProductsQuery ToGetProductsQuery(this GetProductsRequest request)
        {
            return new GetProductsQuery(request.PageNumber, request.PageSize);
        }

        public static GetProductsResponse ToGetProductsResponse(this GetProductsResult result)
        {
            return new GetProductsResponse(result.Products);
        }
    }
}
