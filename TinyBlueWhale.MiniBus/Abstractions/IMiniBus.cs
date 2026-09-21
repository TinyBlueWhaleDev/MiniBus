
namespace TinyBlueWhale.MiniBus.Abstractions
{
    /// <summary>
    /// Defines the contract for dispatching requests and publishing events.
    /// </summary>
    public interface IMiniBus
    {
        /// <summary>
        /// Sends a request to its registered handler.
        /// </summary>
        /// <typeparam name="TResponse">
        /// The response type produced by the request handler.
        /// </typeparam>
        /// <param name="request">
        /// The request to dispatch.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation and contains the
        /// response produced by the request handler.
        /// </returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes an event to all registered handlers.
        /// </summary>
        /// <typeparam name="TEvent">
        /// The event type to publish.
        /// </typeparam>
        /// <param name="event">
        /// The event to publish.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous publishing operation.
        /// </returns>
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
    }
}
