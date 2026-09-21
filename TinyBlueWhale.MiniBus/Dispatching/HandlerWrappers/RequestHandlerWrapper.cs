using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using TinyBlueWhale.MiniBus.Abstractions;

namespace TinyBlueWhale.MiniBus.Dispatching.HandlerWrappers
{
    /// <summary>
    /// Defines the runtime invocation contract for a request handler.
    /// </summary>
    internal abstract class RequestHandlerWrapper
    {
        /// <summary>
        /// Creates a request handler wrapper for the specified handler contract.
        /// </summary>
        /// <param name="handlerType">
        /// The concrete request handler implementation type.
        /// </param>
        /// <param name="requestType">
        /// The request type handled by the handler.
        /// </param>
        /// <param name="responseType">
        /// The response type produced by the handler.
        /// </param>
        /// <returns>
        /// A request handler wrapper for the specified types.
        /// </returns>
        internal static RequestHandlerWrapper Create(Type handlerType, Type requestType, Type responseType)
        {
            var wrapperType = typeof(RequestHandlerWrapper<,,>)
                .MakeGenericType(handlerType, requestType, responseType);

            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        }

        /// <summary>
        /// Invokes the request handler associated with the specified request.
        /// </summary>
        /// <typeparam name="TResponse">
        /// The expected response type.
        /// </typeparam>
        /// <param name="serviceProvider">
        /// The service provider used to resolve the request handler.
        /// </param>
        /// <param name="request">
        /// The request instance to handle.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to propagate cancellation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation and contains the
        /// response produced by the request handler.
        /// </returns>
        internal abstract Task<TResponse> Handle<TResponse>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides strongly typed runtime invocation for a request handler.
    /// </summary>
    /// <typeparam name="THandler">
    /// The concrete request handler implementation type.
    /// </typeparam>
    /// <typeparam name="TRequest">
    /// The request type handled by the handler.
    /// </typeparam>
    /// <typeparam name="TResponse">
    /// The response type produced by the handler.
    /// </typeparam>
    internal sealed class RequestHandlerWrapper<THandler, TRequest, TResponse> : RequestHandlerWrapper
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        internal override Task<TResult> Handle<TResult>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetRequiredService<THandler>();
            var task = handler.Handle((TRequest)request, cancellationToken);

            return (Task<TResult>)(object)task;
        }
    }
}
