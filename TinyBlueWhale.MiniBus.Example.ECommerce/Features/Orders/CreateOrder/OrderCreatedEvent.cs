namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed record OrderCreatedEvent(Guid OrderId,string CustomerEmail,decimal TotalAmount);
}
