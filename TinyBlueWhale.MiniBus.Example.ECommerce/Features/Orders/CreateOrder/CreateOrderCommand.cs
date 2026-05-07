namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed record CreateOrderCommand(string CustomerEmail, decimal TotalAmount) : IRequest<Guid>;
}
