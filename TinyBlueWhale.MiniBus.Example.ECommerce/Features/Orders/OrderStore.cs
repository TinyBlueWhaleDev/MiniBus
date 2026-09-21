using System.Collections.Concurrent;

namespace TinyBlueWhale.MiniBus.Example.ECommerce.Features.Orders
{
    public sealed class OrderStore
    {
        private readonly ConcurrentDictionary<Guid, Order> _orders = [];

        public void Add(Order order)
        {
            _orders[order.Id] = order;
        }

        public Order? Get(Guid id)
        {
            return _orders.GetValueOrDefault(id);
        }
    }
}
