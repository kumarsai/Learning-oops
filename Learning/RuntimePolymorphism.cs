public abstract class DiscountPolicy
{
    public abstract decimal CalculateDiscount(decimal total);
}

public sealed class RegularCustomerDiscount : DiscountPolicy
{
    public override decimal CalculateDiscount(decimal total)
    {
        return total * 0.05m;
    }
}

public sealed class PremiumCustomerDiscount : DiscountPolicy
{
    public override decimal CalculateDiscount(decimal total)
    {
        return total * 0.15m;
    }
}