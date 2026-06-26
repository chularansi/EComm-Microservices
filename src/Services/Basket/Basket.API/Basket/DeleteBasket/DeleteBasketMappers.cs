namespace Basket.API.Basket.DeleteBasket
{
    public static class DeleteBasketMappers
    {
        //public static DeleteBasketCommand ToDeleteBasketCommand(this string userName)
        //{
        //    return new DeleteBasketCommand(userName);
        //}

        public static DeleteBasketResponse ToDeleteBasketResponse(this DeleteBasketResult result)
        {
            return new DeleteBasketResponse(result.IsSuccess);
        }
    }
}
