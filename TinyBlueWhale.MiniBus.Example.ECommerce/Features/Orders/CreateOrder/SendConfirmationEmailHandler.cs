namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed class SendConfirmationEmailHandler : IEventHandler<OrderCreatedEvent>
    {
        public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);

            Console.WriteLine($"Confirmation email sent to '{@event.CustomerEmail}'.");
        }
    }
}
