namespace OrderFlow.Application.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var started = DateTime.UtcNow;

        logger.LogInformation("Handling request {RequestName}", requestName);

        var response = await next();

        var elapsed = DateTime.UtcNow - started;
        logger.LogInformation("Handled request {RequestName} in {ElapsedMs} ms", requestName, elapsed.TotalMilliseconds);

        return response;
    }
}
