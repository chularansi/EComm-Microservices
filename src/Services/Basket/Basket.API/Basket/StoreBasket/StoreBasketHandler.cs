using Discount.Grpc;

namespace Basket.API.Basket.StoreBasket
{
    public class StoreBasketCommandHandler
        (IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProto)
        : ICommandHandler<StoreBasketCommand, StoreBasketResult>
    {
        public async ValueTask<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
        {
            await DeductDiscount(command.Cart, cancellationToken);

            await repository.StoreBasket(command.Cart, cancellationToken);

            return new StoreBasketResult(command.Cart.UserName);
        }

        private async Task DeductDiscount(ShoppingCart cart, CancellationToken cancellationToken)
        {
            // Communicate with Discount.Grpc and calculate lastest prices of products into sc
            //foreach (var item in cart.Items)
            //{
            //    var coupon = await discountProto.GetDiscountAsync(new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: cancellationToken);
            //    item.Price -= coupon.Amount;
            //}

            // Fix 1: Trigger all gRPC calls in parallel instead of waiting sequentially
            var discountTasks = cart.Items.Select(async item =>
            {
                var coupon = await discountProto.GetDiscountAsync(
                    new GetDiscountRequest { ProductName = item.ProductName },
                    cancellationToken: cancellationToken
                );

                // Fix 2: Reset/Ensure the calculation relies on a reliable Catalog/Base Price
                // To prevent the "infinite subtraction bug", ensure item payloads include an unmutated BasePrice field
                // Alternatively, resolve the base price from a product catalog service here first.
                item.Price = item.BasePrice - coupon.Amount;
            });

            await Task.WhenAll(discountTasks);
        }
    }
}
