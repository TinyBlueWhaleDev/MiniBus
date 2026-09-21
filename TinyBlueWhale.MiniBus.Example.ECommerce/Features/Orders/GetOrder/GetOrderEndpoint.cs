using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders.GetOrder
{
    public static class GetOrderEndpoint
    {
        public static IEndpointRouteBuilder MapGetOrderEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/orders/{orderId:guid}", GetOrder)
                .WithName("GetOrder")
                .WithTags("Orders");

            return endpoints;
        }

        private static async Task<IResult> GetOrder(Guid orderId, IMiniBus miniBus, CancellationToken cancellationToken)
        {
            var query = new GetOrderQuery(orderId);

            var order = await miniBus.Send(query, cancellationToken);

            return order is null
                ? Results.NotFound()
                : Results.Ok(order);
        }
    }
}