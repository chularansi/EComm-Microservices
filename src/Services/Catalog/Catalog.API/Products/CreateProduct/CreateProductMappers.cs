namespace Catalog.API.Products.CreateProduct
{
    public static class CreateProductMappers
    {
        public static CreateProductCommand ToCreateProductCommand(this CreateProductRequest request)
        {
            return new CreateProductCommand(request.Name, request.Category, request.Description, request.ImageFile, request.Price);
        }
    
        public static CreateProductResponse ToCreateProductResponse(this CreateProductResult result)
        {
            return new CreateProductResponse(result.Id);
        }
    }
}
