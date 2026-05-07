using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TinyBlueWhale.MiniBus
{

    #region Public Contracts
    /// <summary>
    /// Minimal in-process bus for request/response dispatching and event publishing.
    /// Designed for the common 80% use case: Send request, publish event.
    /// </summary>    
    public interface IMiniBus
    {
        Task<TResponse> Send<TResponse>(
         IRequest<TResponse> request,
         CancellationToken cancellationToken = default);

        Task Publish<TEvent>(
            TEvent @event,
            CancellationToken cancellationToken = default);
    }    

    /// <summary>
    /// Marker interface for request/response messages.
    /// Example: public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;
    /// </summary>
    public interface IRequest<out TResponse>;

    /// <summary>
    /// Handles a single request type and returns a response.
    /// Only one handler per request type is allowed.
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Handles an event/notification.
    /// Multiple handlers per event type are allowed.
    /// </summary>
    public interface IEventHandler<in TEvent>
    {
        Task Handle(TEvent @event, CancellationToken cancellationToken = default);
    }

    #endregion

    #region MiniBus Implementation

    /// <summary>
    /// Runtime implementation of IMiniBus.
    /// 
    /// Design notes:
    /// - Scoped lifetime, so handlers can safely depend on scoped services.
    /// - Handler lookup is done using a prebuilt registry.
    /// - No dynamic dispatch.
    /// - No expression compilation on the hot path.
    /// </summary>

    internal sealed class MiniBus(IServiceProvider serviceProvider, MiniBusRegistry registry) : IMiniBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly MiniBusRegistry _registry = registry;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var requestType = request.GetType();

            if (!_registry.Requests.TryGetValue(requestType, out var wrapper))
                throw new InvalidOperationException($"No handler registered for request '{requestType.FullName}'.");

            return wrapper.Handle<TResponse>(_serviceProvider, request, cancellationToken);
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var eventType = @event.GetType();

            if (!_registry.Events.TryGetValue(eventType, out var wrappers))
                return;

            // SR: Sequential publishing is intentional.
            // It avoids Task[] allocation and keeps the default path predictable.
            // A future PublishParallel method can be added for IO-heavy fanout scenarios.
            for (var i = 0; i < wrappers.Length; i++)
                await wrappers[i].Handle(_serviceProvider, @event, cancellationToken).ConfigureAwait(false);
            
        }
    }

    #endregion

    #region Handler Registry

    /// <summary>
    /// Precomputed handler registry.
    /// 
    /// SR: This object is singleton and immutable after startup.
    /// It stores metadata only, not scoped handler instances.
    /// </summary>
    internal sealed class MiniBusRegistry
    {
        public required IReadOnlyDictionary<Type, RequestHandlerWrapper> Requests { get; init; }

        public required IReadOnlyDictionary<Type, EventHandlerWrapper[]> Events { get; init; }
    }

    #endregion

    #region Request Handler Wrappers

    /// <summary>
    /// Non-generic base wrapper used by the registry.
    /// Converts runtime request lookup into a strongly typed handler call.
    /// </summary>
    internal abstract class RequestHandlerWrapper
    {
        public abstract Task<TResponse> Handle<TResponse>(IServiceProvider serviceProvider,object request,CancellationToken cancellationToken);
    }

    /// <summary>
    /// Strongly typed wrapper for IRequestHandler.
    /// 
    /// SR: The Unsafe.As cast avoids extra reflection/dynamic dispatch.
    /// It is safe because this wrapper is created from IRequestHandler<TRequest, TResponse>
    /// during registration.
    /// </summary>
    internal sealed class RequestHandlerWrapper<THandler, TRequest, TResponse> : RequestHandlerWrapper
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TRes> Handle<TRes>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<THandler>();
            var task = handler.Handle((TRequest)request, cancellationToken);

            return Unsafe.As<Task<TResponse>, Task<TRes>>(ref task);
        }
    }

    #endregion

    #region Event Handler Wrappers

    /// <summary>
    /// Non-generic base wrapper used by the registry.
    /// Allows multiple event handlers per event type.
    /// </summary>
    internal abstract class EventHandlerWrapper
    {
        public abstract Task Handle(IServiceProvider serviceProvider,object @event, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Strongly typed wrapper for IEventHandler.
    /// </summary>
    internal sealed class EventHandlerWrapper<THandler, TEvent> : EventHandlerWrapper
        where THandler : IEventHandler<TEvent>
    {
        public override Task Handle(IServiceProvider serviceProvider,object @event,CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<THandler>();
            return handler.Handle((TEvent)@event, cancellationToken);
        }
    }

    #endregion

    #region Dependency Injection Registration
    public static class ServiceCollectionExtensions
    {
        private static readonly Type RequestHandlerDefinition = typeof(IRequestHandler<,>);
        private static readonly Type EventHandlerDefinition = typeof(IEventHandler<>);

        /// <summary>
        /// Registers MiniBus and scans the provided assemblies for request/event handlers.
        /// </summary>
        public static IServiceCollection AddMiniBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (assemblies.Length == 0)
                assemblies = [Assembly.GetCallingAssembly()];

            var requests = new Dictionary<Type, RequestHandlerWrapper>();
            var events = new Dictionary<Type, List<EventHandlerWrapper>>();

            var handlerTypes = assemblies
                .Distinct()
                .SelectMany(static assembly => assembly.DefinedTypes)
                .Where(static type =>
                    type is { IsAbstract: false, IsInterface: false, IsClass: true })
                .ToArray();

            foreach (var handlerInfo in handlerTypes)
            {
                var handlerType = handlerInfo.AsType();
                var implementedInterfaces = handlerType.GetInterfaces();

                var hasHandlerInterface = false;

                foreach (var iface in implementedInterfaces)
                {
                    if (!iface.IsGenericType)
                        continue;

                    var definition = iface.GetGenericTypeDefinition();

                    if (definition == RequestHandlerDefinition)
                    {
                        hasHandlerInterface = true;

                        var requestType = iface.GenericTypeArguments[0];
                        var responseType = iface.GenericTypeArguments[1];

                        if (requests.ContainsKey(requestType))
                            throw new InvalidOperationException($"Multiple request handlers registered for '{requestType.FullName}'.");

                        requests[requestType] = CreateRequestWrapper(handlerType,requestType,responseType);
                    }
                    else if (definition == EventHandlerDefinition)
                    {
                        hasHandlerInterface = true;

                        var eventType = iface.GenericTypeArguments[0];

                        if (!events.TryGetValue(eventType, out var list))
                        {
                            list = [];
                            events[eventType] = list;
                        }

                        list.Add(CreateEventWrapper(handlerType, eventType));
                    }
                }

                if (hasHandlerInterface)
                    services.TryAddTransient(handlerType);
            }

            var registry = new MiniBusRegistry
            {
                Requests = requests,
                Events = events.ToDictionary(
                    static x => x.Key,
                    static x => x.Value.ToArray())
            };

            services.TryAddSingleton(registry);
            services.TryAddScoped<IMiniBus, MiniBus>();

            return services;
        }

        #endregion

        #region Wrapper Factory Methods
        /// <summary>
        /// Creates a strongly typed request handler wrapper.
        /// 
        /// SR: The wrapper is created once during startup and reused through the registry.
        /// This keeps Send free of reflection and dynamic dispatch.
        /// </summary>
        private static RequestHandlerWrapper CreateRequestWrapper(Type handlerType, Type requestType, Type responseType)
        {
            var wrapperType = typeof(RequestHandlerWrapper<,,>)
                .MakeGenericType(handlerType, requestType, responseType);

            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        }

        /// <summary>
        /// Creates a strongly typed event handler wrapper.
        /// 
        /// SR: The wrapper is created once during startup and reused through the registry.
        /// This keeps Publish free of reflection and dynamic dispatch.
        /// </summary>
        private static EventHandlerWrapper CreateEventWrapper(Type handlerType, Type eventType)
        {
            var wrapperType = typeof(EventHandlerWrapper<,>)
                .MakeGenericType(handlerType, eventType);

            return (EventHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        }

        #endregion
    }
}