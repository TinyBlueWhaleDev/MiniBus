namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders
{
    public sealed record Order(Guid Id, string CustomerEmail, decimal TotalAmount);
}
