using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed class CreateOrderHandler(IMiniBus miniBus, OrderStore orderStore) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(20, cancellationToken);

            var order = new Order(
                Guid.NewGuid(),
                request.CustomerEmail,
                request.TotalAmount);

            orderStore.Add(order);

            await miniBus.Publish(
                new OrderCreatedEvent(
                    order.Id,
                    order.CustomerEmail,
                    order.TotalAmount),
                cancellationToken);

            return order.Id;
        }
    }    
}
