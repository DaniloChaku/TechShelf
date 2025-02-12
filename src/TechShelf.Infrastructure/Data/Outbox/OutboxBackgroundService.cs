using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TechShelf.Infrastructure.Data.Outbox;

public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly int _outboxProcessorFrequencyMilliseconds;

    public OutboxBackgroundService(IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _outboxProcessorFrequencyMilliseconds = options.Value.OutboxProcessorFrequencyMilliseconds;
        _logger = logger;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", 
        "S6667:Logging in a catch clause should pass the caught exception as a parameter.")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started OutboxBackgroundService...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxMessageProcessor>();

                await outboxProcessor.ExecuteAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMilliseconds(_outboxProcessorFrequencyMilliseconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox background service cancelled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "An error occurred in the Outbox background service. Retrying in {Seconds} seconds...",
                    _outboxProcessorFrequencyMilliseconds);
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(_outboxProcessorFrequencyMilliseconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
