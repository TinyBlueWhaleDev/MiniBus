namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed class UpdateInventoryHandler
    : IEventHandler<OrderCreatedEvent>
    {
        public async Task Handle(OrderCreatedEvent @event,CancellationToken cancellationToken = default)
        {
            await Task.Delay(5, cancellationToken);

            Console.WriteLine(
                $"Inventory updated for order '{@event.OrderId}'.");
        }
    }
}
