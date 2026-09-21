using System.Reflection;
using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Discovery
{
    /// <summary>
    /// Discovers MiniBus handlers from application assemblies.
    /// </summary>
    internal static class HandlerDiscovery
    {
        private static readonly Type RequestHandlerType = typeof(IRequestHandler<,>);
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);

        /// <summary>
        /// Discovers request and event handlers from the specified assemblies.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies to inspect for MiniBus handlers.
        /// </param>
        /// <returns>
        /// The request and event handlers discovered from the specified assemblies.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="assemblies"/> is <see langword="null"/>.
        /// </exception>
        internal static HandlerDiscoveryResult Discover(IEnumerable<Assembly> assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);

            var handlers = assemblies
                .Distinct()
                .SelectMany(static assembly => assembly.DefinedTypes)
                .Where(static type => type is { IsClass: true, IsAbstract: false })
                .SelectMany(static type => type
                    .GetInterfaces()
                    .Where(static contract => contract.IsGenericType)
                    .Select(contract => new
                    {
                        HandlerType = type.AsType(),
                        ContractType = contract,
                        ContractDefinition = contract.GetGenericTypeDefinition()
                    }))
                .ToArray();


            var requestHandlers = handlers
                .Where(handler => handler.ContractDefinition == RequestHandlerType)
                .Select(static handler =>
                {
                    var arguments = handler.ContractType.GetGenericArguments();
                    return new RequestHandlerDescriptor(handler.HandlerType, arguments[0], arguments[1]);
                }).ToArray();

            var eventHandlers = handlers
                .Where(handler => handler.ContractDefinition == EventHandlerType)
                .Select(static handler =>
                {
                    var arguments = handler.ContractType.GetGenericArguments();
                    return new EventHandlerDescriptor(handler.HandlerType, arguments[0]);
                }).ToArray();

            return new HandlerDiscoveryResult(requestHandlers, eventHandlers);
        }
    }
}
