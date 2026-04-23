namespace OrderFlow.Infrastructure.Services;

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Interfaces;

public sealed class OutboxProcessorService(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessorService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pendingMessages = await outboxRepository.GetPendingAsync(50, stoppingToken);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        if (message.EventType == nameof(OrderPlacedEvent))
                        {
                            var orderPlacedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(message.Payload);
                            if (orderPlacedEvent is not null)
                            {
                                await publisher.Publish(orderPlacedEvent, stoppingToken);
                            }
                        }

                        message.MarkProcessed();
                    }
                    catch (Exception ex)
                    {
                        message.MarkFailedAttempt();
                        logger.LogError(ex, "Failed to process outbox message {OutboxMessageId}.", message.Id);
                    }
                }

                if (pendingMessages.Count > 0)
                {
                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processing loop failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
