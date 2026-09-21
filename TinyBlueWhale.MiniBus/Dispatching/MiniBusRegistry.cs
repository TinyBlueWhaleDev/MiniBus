using TinyBlueWhale.MiniBus.Dispatching.HandlerWrappers;

namespace TinyBlueWhale.MiniBus.Dispatching
{
    /// <summary>
    /// Stores the request and event handler mappings used during message dispatch.
    /// </summary>
    /// <param name="requests">
    /// The request handler wrappers indexed by request type.
    /// </param>
    /// <param name="events">
    /// The event handler wrappers indexed by event type.
    /// </param>
    internal sealed class MiniBusRegistry(IReadOnlyDictionary<Type, RequestHandlerWrapper> requests, IReadOnlyDictionary<Type, List<EventHandlerWrapper>> events)
    {
        private readonly IReadOnlyDictionary<Type, RequestHandlerWrapper> _requests = requests;
        private readonly IReadOnlyDictionary<Type, List<EventHandlerWrapper>> _events = events;

        /// <summary>
        /// Gets the request handler wrapper registered for the specified request type.
        /// </summary>
        /// <param name="requestType">
        /// The request type used to locate the handler.
        /// </param>
        /// <returns>
        /// The request handler wrapper registered for the specified request type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no request handler is registered for the specified request type.
        /// </exception>
        internal RequestHandlerWrapper GetRequestHandler(Type requestType)
        {
            if (_requests.TryGetValue(requestType, out var handler))
                return handler;

            throw new InvalidOperationException($"No request handler registered for '{requestType.FullName}'.");
        }

        /// <summary>
        /// Gets the event handler wrappers registered for the specified event type.
        /// </summary>
        /// <param name="eventType">
        /// The event type used to locate the handlers.
        /// </param>
        /// <returns>
        /// The event handler wrappers registered for the specified event type,
        /// or an empty collection when no handlers are registered.
        /// </returns>
        internal IReadOnlyList<EventHandlerWrapper> GetEventHandlers(Type eventType)
        {
            return _events.TryGetValue(eventType, out var handlers)
                ? handlers
                : [];
        }
    }
}
