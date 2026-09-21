using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using TinyBlueWhale.MiniBus.Abstractions;
using TinyBlueWhale.MiniBus.Discovery;
using TinyBlueWhale.MiniBus.Dispatching;
using TinyBlueWhale.MiniBus.Dispatching.HandlerWrappers;

namespace TinyBlueWhale.MiniBus.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering MiniBus services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MiniBus services and discovers handlers from the specified assemblies.
        /// </summary>
        /// <param name="services">
        /// The service collection used to register MiniBus services.
        /// </param>
        /// <param name="assemblies">
        /// The assemblies to inspect for MiniBus handlers.
        /// </param>
        /// <returns>
        /// The service collection so that additional calls can be chained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="services"/> or <paramref name="assemblies"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when multiple request handlers are discovered for the same request type.
        /// </exception>
        public static IServiceCollection AddMiniBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblies);

            var discovery = HandlerDiscovery.Discover(assemblies);

            var requests = CreateRequestHandlers(discovery.RequestHandlers);
            var events = CreateEventHandlers(discovery.EventHandlers);

            RegisterHandlers(services, discovery);

            var registry = new MiniBusRegistry(requests, events);

            services.TryAddSingleton(registry);
            services.TryAddScoped<IMiniBus, MiniBus>();

            return services;
        }

        /// <summary>
        /// Creates the request handler mappings from the discovered request handlers.
        /// </summary>
        /// <param name="handlers">
        /// The discovered request handlers.
        /// </param>
        /// <returns>
        /// The request handler wrappers indexed by request type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when multiple handlers are discovered for the same request type.
        /// </exception>
        private static Dictionary<Type, RequestHandlerWrapper> CreateRequestHandlers(IReadOnlyCollection<RequestHandlerDescriptor> handlers)
        {
            var requests = new Dictionary<Type, RequestHandlerWrapper>();

            foreach (var handler in handlers)
                if (!requests.TryAdd(handler.RequestType, RequestHandlerWrapper.Create(handler.HandlerType, handler.RequestType, handler.ResponseType)))
                    throw new InvalidOperationException($"Multiple request handlers registered for '{handler.RequestType.FullName}'.");

            return requests;
        }

        /// <summary>
        /// Creates the event handler mappings from the discovered event handlers.
        /// </summary>
        /// <param name="handlers">
        /// The discovered event handlers.
        /// </param>
        /// <returns>
        /// The event handler wrappers grouped by event type.
        /// </returns>
        private static Dictionary<Type, List<EventHandlerWrapper>> CreateEventHandlers(IReadOnlyCollection<EventHandlerDescriptor> handlers)
        {
            var events = new Dictionary<Type, List<EventHandlerWrapper>>();

            foreach (var handler in handlers)
            {
                if (!events.TryGetValue(handler.EventType, out var eventHandlers))
                {
                    eventHandlers = [];
                    events[handler.EventType] = eventHandlers;
                }

                eventHandlers.Add(EventHandlerWrapper.Create(handler.HandlerType, handler.EventType));
            }

            return events;
        }

        /// <summary>
        /// Registers the discovered handler implementation types in the service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection used to register handler implementations.
        /// </param>
        /// <param name="discovery">
        /// The discovered request and event handlers.
        /// </param>
        private static void RegisterHandlers(IServiceCollection services, HandlerDiscoveryResult discovery)
        {
            var handlerTypes = discovery.RequestHandlers
                .Select(static handler => handler.HandlerType)
                .Concat(discovery.EventHandlers.Select(
                    static handler => handler.HandlerType))
                .Distinct();

            foreach (var handlerType in handlerTypes)
                services.TryAddTransient(handlerType);
        }
    }
}
