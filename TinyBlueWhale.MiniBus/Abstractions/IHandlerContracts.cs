
namespace TinyBlueWhale.MiniBus.Abstractions
{
    /// <summary>
    /// Defines a request that produces a response.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The response type produced by the request.
    /// </typeparam>
    public interface IRequest<out TResponse>;

    /// <summary>
    /// Defines a handler for a request that produces a response.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The request type handled by the handler.
    /// </typeparam>
    /// <typeparam name="TResponse">
    /// The response type produced by the handler.
    /// </typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">
        /// The request to handle.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation and contains the
        /// response produced by the handler.
        /// </returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines a handler for an event.
    /// </summary>
    /// <typeparam name="TEvent">
    /// The event type handled by the handler.
    /// </typeparam>
    public interface IEventHandler<in TEvent>
    {
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">
        /// The event to handle.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous event handling operation.
        /// </returns>
        Task Handle(TEvent @event, CancellationToken cancellationToken);
    }
}
