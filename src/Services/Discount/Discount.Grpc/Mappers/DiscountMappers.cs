using Discount.Grpc.Models;

namespace Discount.Grpc.Mappers
{
    public static class DiscountMappers
    {
        public static CouponModel ToCouponModel(this Coupon coupon)
        {
            return new CouponModel
            {
                Id = coupon.Id,
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = coupon.Amount,
            };
        }

        public static Coupon ToCoupon(this CouponModel couponModel)
        {
            return new Coupon
            {
                Id = couponModel.Id,
                ProductName = couponModel.ProductName,
                Description = couponModel.Description,
                Amount = couponModel.Amount,
            };
        }
    }
}
