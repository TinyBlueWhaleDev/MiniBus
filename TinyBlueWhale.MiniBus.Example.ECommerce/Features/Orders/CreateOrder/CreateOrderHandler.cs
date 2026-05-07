namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public sealed class CreateOrderHandler(IMiniBus miniBus)
        : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IMiniBus _miniBus = miniBus;

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(20, cancellationToken);

            var orderId = Guid.NewGuid();

            await _miniBus.Publish(
                new OrderCreatedEvent(
                    orderId,
                    request.CustomerEmail,
                    request.TotalAmount),
                cancellationToken);

            return orderId;
        }
    }
}
