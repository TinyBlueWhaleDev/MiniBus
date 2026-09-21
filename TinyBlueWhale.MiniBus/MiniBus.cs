using TinyBlueWhale.MiniBus.Abstractions;
using TinyBlueWhale.MiniBus.Dispatching;

namespace TinyBlueWhale.MiniBus
{
    /// <summary>
    /// Dispatches requests and publishes events through the registered MiniBus handlers.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider used to resolve registered handler instances.
    /// </param>
    /// <param name="registry">
    /// The registry used to locate request and event handler wrappers.
    /// </param>
    internal sealed class MiniBus(IServiceProvider serviceProvider, MiniBusRegistry registry) : IMiniBus
    {
        /// <inheritdoc />
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var wrapper = registry.GetRequestHandler(request.GetType());

            return wrapper.Handle<TResponse>(serviceProvider, request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var wrappers = registry.GetEventHandlers(@event.GetType());

            foreach (var wrapper in wrappers)
                await wrapper.Handle(serviceProvider, @event, cancellationToken).ConfigureAwait(false);
        }
    }
}