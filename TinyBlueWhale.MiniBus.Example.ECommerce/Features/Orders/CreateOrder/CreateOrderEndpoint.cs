namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.CreateOrder
{
    public static class CreateOrderEndpoint
    {
        public static IEndpointRouteBuilder MapCreateOrderEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/orders", CreateOrder);

            return endpoints;
        }

        private static async Task<IResult> CreateOrder(CreateOrderRequest request,IMiniBus miniBus, CancellationToken cancellationToken)
        {
            var command = new CreateOrderCommand(
                request.CustomerEmail,
                request.TotalAmount);

            var orderId = await miniBus.Send(command, cancellationToken);

            return Results.Created($"/orders/{orderId}", new
            {
                OrderId = orderId
            });
        }
    }

    public sealed record CreateOrderRequest(
        string CustomerEmail,
        decimal TotalAmount);
}
