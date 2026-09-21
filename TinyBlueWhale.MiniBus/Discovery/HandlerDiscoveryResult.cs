
namespace TinyBlueWhale.MiniBus.Discovery
{
    /// <summary>
    /// Represents the handlers discovered from the configured assemblies.
    /// </summary>
    /// <param name="RequestHandlers">
    /// The discovered request handlers.
    /// </param>
    /// <param name="EventHandlers">
    /// The discovered event handlers.
    /// </param>
    internal sealed record HandlerDiscoveryResult(IReadOnlyCollection<RequestHandlerDescriptor> RequestHandlers, IReadOnlyCollection<EventHandlerDescriptor> EventHandlers);

    /// <summary>
    /// Describes a discovered request handler.
    /// </summary>
    /// <param name="HandlerType">
    /// The concrete handler implementation type.
    /// </param>
    /// <param name="RequestType">
    /// The request type handled by the handler.
    /// </param>
    /// <param name="ResponseType">
    /// The response type produced by the handler.
    /// </param>
    internal sealed record RequestHandlerDescriptor(Type HandlerType, Type RequestType, Type ResponseType);

    /// <summary>
    /// Describes a discovered event handler.
    /// </summary>
    /// <param name="HandlerType">
    /// The concrete handler implementation type.
    /// </param>
    /// <param name="EventType">
    /// The event type handled by the handler.
    /// </param>
    internal sealed record EventHandlerDescriptor(Type HandlerType, Type EventType);
}
