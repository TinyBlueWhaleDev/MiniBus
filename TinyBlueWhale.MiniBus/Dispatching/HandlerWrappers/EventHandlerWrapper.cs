using Microsoft.Extensions.DependencyInjection;
using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Dispatching.HandlerWrappers
{
    /// <summary>
    /// Defines the runtime invocation contract for an event handler.
    /// </summary>
    internal abstract class EventHandlerWrapper
    {
        /// <summary>
        /// Creates an event handler wrapper for the specified handler contract.
        /// </summary>
        /// <param name="handlerType">
        /// The concrete event handler implementation type.
        /// </param>
        /// <param name="eventType">
        /// The event type handled by the handler.
        /// </param>
        /// <returns>
        /// An event handler wrapper for the specified types.
        /// </returns>
        internal static EventHandlerWrapper Create(Type handlerType, Type eventType)
        {
            var wrapperType = typeof(EventHandlerWrapper<,>)
                .MakeGenericType(handlerType, eventType);

            return (EventHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        }

        /// <summary>
        /// Invokes the event handler associated with the specified event.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve the event handler.
        /// </param>
        /// <param name="event">
        /// The event instance to handle.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous event handling operation.
        /// </returns>
        internal abstract Task Handle(IServiceProvider serviceProvider, object @event, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides strongly typed runtime invocation for an event handler.
    /// </summary>
    /// <typeparam name="THandler">
    /// The concrete event handler implementation type.
    /// </typeparam>
    /// <typeparam name="TEvent">
    /// The event type handled by the handler.
    /// </typeparam>
    internal sealed class EventHandlerWrapper<THandler, TEvent> : EventHandlerWrapper
        where THandler : IEventHandler<TEvent>
    {
        internal override Task Handle(IServiceProvider serviceProvider, object @event, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<THandler>();

            return handler.Handle((TEvent)@event, cancellationToken);
        }
    }
}
