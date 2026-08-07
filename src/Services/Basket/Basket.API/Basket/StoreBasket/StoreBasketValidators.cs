namespace Basket.API.Basket.StoreBasket
{
    public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketCommandValidator()
        {
            //RuleFor(x => x.Cart).NotNull().WithMessage("Cart can not be null");
            //RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("UserName is required");

            // 1. Check if Cart is null first
            RuleFor(x => x.Cart)
                .NotNull().WithMessage("Cart can not be null")
                .DependentRules(() =>
                {
                    // 2. These rules only run if Cart is definitely NOT null
                    RuleFor(x => x.Cart.UserName)
                        .NotEmpty().WithMessage("UserName is required");
                });
        }
    }
}
