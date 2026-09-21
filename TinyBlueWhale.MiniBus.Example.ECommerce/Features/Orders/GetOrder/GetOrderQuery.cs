using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.GetOrder
{
    public sealed record GetOrderQuery(Guid OrderId) : IRequest<GetOrderResponse?>;
    public sealed record GetOrderResponse(Guid OrderId, string CustomerEmail, decimal TotalAmount);
}
