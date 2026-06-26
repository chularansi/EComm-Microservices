using Catalog.API.Products.CreateProduct;

namespace Catalog.API.Products.UpdateProduct
{
    public static class UpdateProductMappers
    {
        //UpdateProductCommand
        public static UpdateProductCommand ToUpdateProductCommand(this UpdateProductRequest request)
        {
            return new UpdateProductCommand(request.Id, request.Name, request.Category, request.Description, request.ImageFile, request.Price);
        }

        public static UpdateProductResponse ToUpdateProductResponse(this UpdateProductResult result)
        {
            return new UpdateProductResponse(result.IsSuccess);
        }
    }
}
