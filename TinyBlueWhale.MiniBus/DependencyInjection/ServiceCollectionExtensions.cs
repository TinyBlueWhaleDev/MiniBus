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
        /// Registers MiniBus and discovers request and event handlers from the specified assemblies.
        /// </summary>
        /// <param name="services">
        /// The service collection where MiniBus services are registered.
        /// </param>
        /// <param name="assemblies">
        /// The assemblies to scan for request and event handlers.
        /// </param>
        /// <returns>
        /// The same service collection instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="services"/> or <paramref name="assemblies"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when no assemblies are provided.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when multiple request handlers are registered for the same request type.
        /// </exception>
        public static IServiceCollection AddMiniBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblies);

            if (assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            if (assemblies.Any(assembly => assembly is null))
                throw new ArgumentException("Assemblies cannot contain null values.", nameof(assemblies));

            var registry = GetOrCreateRegistry(services);

            var unregisteredAssemblies = assemblies
                .Distinct()
                .Where(assembly => !registry.IsAssemblyRegistered(assembly))
                .ToArray();

            if (unregisteredAssemblies.Length == 0)
            {
                services.TryAddScoped<IMiniBus, MiniBus>();

                return services;
            }

            var discovery = HandlerDiscovery.Discover(unregisteredAssemblies);

            RegisterRequestHandlers(registry, discovery.RequestHandlers);
            RegisterEventHandlers(registry, discovery.EventHandlers);
            RegisterHandlers(services, discovery);

            foreach (var assembly in unregisteredAssemblies)
            {
                registry.RegisterAssembly(assembly);
            }

            services.TryAddScoped<IMiniBus, MiniBus>();

            return services;
        }

        /// <summary>
        /// Gets the existing MiniBus registry or creates and registers a new one.
        /// </summary>
        /// <param name="services">
        /// The service collection containing the MiniBus registrations.
        /// </param>
        /// <returns>
        /// The MiniBus registry associated with the service collection.
        /// </returns>
        private static MiniBusRegistry GetOrCreateRegistry(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(service => service.ServiceType == typeof(MiniBusRegistry));

            if (descriptor?.ImplementationInstance is MiniBusRegistry registry)
                return registry;

            registry = new MiniBusRegistry();

            services.AddSingleton(registry);

            return registry;
        }

        /// <summary>
        /// Registers discovered request handler wrappers in the MiniBus registry.
        /// </summary>
        /// <param name="registry">
        /// The registry where request handlers are stored.
        /// </param>
        /// <param name="handlers">
        /// The discovered request handlers to register.
        /// </param>
        private static void RegisterRequestHandlers(MiniBusRegistry registry, IReadOnlyCollection<RequestHandlerDescriptor> handlers)
        {
            foreach (var handler in handlers)
                registry.RegisterRequestHandler(handler.RequestType, RequestHandlerWrapper.Create(handler.HandlerType, handler.RequestType, handler.ResponseType));
        }

        /// <summary>
        /// Registers discovered event handler wrappers in the MiniBus registry.
        /// </summary>
        /// <param name="registry">
        /// The registry where event handlers are stored.
        /// </param>
        /// <param name="handlers">
        /// The discovered event handlers to register.
        /// </param>
        private static void RegisterEventHandlers(MiniBusRegistry registry, IReadOnlyCollection<EventHandlerDescriptor> handlers)
        {
            foreach (var handler in handlers)
                registry.RegisterEventHandler(handler.EventType, EventHandlerWrapper.Create(handler.HandlerType, handler.EventType));
        }

        /// <summary>
        /// Registers discovered handler implementation types in the service collection.
        /// </summary>
        /// <param name="services">
        /// The service collection where handlers are registered.
        /// </param>
        /// <param name="discovery">
        /// The handler discovery result containing request and event handlers.
        /// </param>
        private static void RegisterHandlers(IServiceCollection services, HandlerDiscoveryResult discovery)
        {
            var handlerTypes = discovery.RequestHandlers
                .Select(handler => handler.HandlerType)
                .Concat(discovery.EventHandlers.Select(handler => handler.HandlerType))
                .Distinct();

            foreach (var handlerType in handlerTypes)
                services.TryAddTransient(handlerType);
        }
    }
}
