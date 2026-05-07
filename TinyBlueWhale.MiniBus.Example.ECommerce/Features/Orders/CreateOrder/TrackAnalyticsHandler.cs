namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed class TrackAnalyticsHandler : IEventHandler<OrderCreatedEvent>
    {
        public async Task Handle(OrderCreatedEvent @event,CancellationToken cancellationToken = default)
        {
            await Task.Delay(3, cancellationToken);

            Console.WriteLine($"Analytics tracked for order '{@event.OrderId}'.");
        }
    }
}
