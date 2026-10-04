using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureLab.Application;

// This tiny dispatcher exposes the mechanics behind a mediator: locate one
// handler for the request type and delegate execution to it. A real project
// may use MediatR when pipeline behaviors and package conventions are useful.
public sealed class RequestDispatcher(IServiceProvider services) : IRequestDispatcher
{
    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));
        object handler = services.GetRequiredService(handlerType);
        MethodInfo handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync))
            ?? throw new InvalidOperationException($"No HandleAsync method exists for {requestType.Name}.");

        object? result = handleMethod.Invoke(
            handler,
            [request, cancellationToken]);

        if (result is not Task<TResponse> responseTask)
        {
            throw new InvalidOperationException(
                $"Handler {handlerType.Name} returned an unexpected response.");
        }

        return await responseTask;
    }
}
