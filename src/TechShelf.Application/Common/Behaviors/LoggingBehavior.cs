using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TechShelf.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}: {Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();

        TResponse response;

        response = await next();

        stopwatch.Stop();

        _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms. Response: {Response}",
            requestName, stopwatch.ElapsedMilliseconds, response);

        return response;
    }
}
