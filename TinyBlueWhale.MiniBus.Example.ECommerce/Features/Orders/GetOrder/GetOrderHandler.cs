using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.GetOrder
{
    public sealed class GetOrderHandler(OrderStore orderStore) : IRequestHandler<GetOrderQuery, GetOrderResponse?>
    {
        public Task<GetOrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken = default)
        {
            var order = orderStore.Get(request.OrderId);

            var response = order is null
                ? null
                : new GetOrderResponse(
                    order.Id,
                    order.CustomerEmail,
                    order.TotalAmount);

            return Task.FromResult(response);
        }
    }
}
