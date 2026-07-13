namespace Catalog.API.Products.GetProductByCategory
{
    public static class GetProductByCategoryMappers
    {
        public static GetProductByCategoryResponse ToProductByCategoryResponse(this GetProductByCategoryResult result)
        {
            return new GetProductByCategoryResponse(result.Products);
        }
    }
}
