using System.Reflection;
using TinyBlueWhale.MiniBus.Dispatching.HandlerWrappers;

namespace TinyBlueWhale.MiniBus.Dispatching
{
    /// <summary>
    /// Stores the runtime mappings between message types and their handler wrappers.
    /// </summary>
    internal sealed class MiniBusRegistry
    {
        private readonly Dictionary<Type, RequestHandlerWrapper> _requests = [];
        private readonly Dictionary<Type, List<EventHandlerWrapper>> _events = [];
        private readonly HashSet<Assembly> _assemblies = [];

        /// <summary>
        /// Determines whether the specified assembly has already been registered.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to inspect.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the assembly has already been registered;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal bool IsAssemblyRegistered(Assembly assembly)
        {
            return _assemblies.Contains(assembly);
        }

        /// <summary>
        /// Marks the specified assembly as registered.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to register.
        /// </param>
        internal void RegisterAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
        }

        /// <summary>
        /// Registers a request handler wrapper for the specified request type.
        /// </summary>
        /// <param name="requestType">
        /// The request type handled by the wrapper.
        /// </param>
        /// <param name="handler">
        /// The request handler wrapper to register.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a handler has already been registered for the request type.
        /// </exception>
        internal void RegisterRequestHandler(Type requestType, RequestHandlerWrapper handler)
        {
            if (!_requests.TryAdd(requestType, handler))
                throw new InvalidOperationException($"Multiple request handlers registered for '{requestType.FullName}'.");            
        }

        /// <summary>
        /// Registers an event handler wrapper for the specified event type.
        /// </summary>
        /// <param name="eventType">
        /// The event type handled by the wrapper.
        /// </param>
        /// <param name="handler">
        /// The event handler wrapper to register.
        /// </param>
        internal void RegisterEventHandler(Type eventType, EventHandlerWrapper handler)
        {
            if (!_events.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _events[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        /// <summary>
        /// Gets the request handler wrapper registered for the specified request type.
        /// </summary>
        /// <param name="requestType">
        /// The request type whose handler should be retrieved.
        /// </param>
        /// <returns>
        /// The registered request handler wrapper.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no handler has been registered for the request type.
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
        /// The event type whose handlers should be retrieved.
        /// </param>
        /// <returns>
        /// The registered event handler wrappers, or an empty collection when no handlers are registered.
        /// </returns>
        internal IReadOnlyList<EventHandlerWrapper> GetEventHandlers(Type eventType)
        {
            return _events.TryGetValue(eventType, out var handlers)
                ? handlers
                : [];
        }
    }
}
