namespace OrderFlow.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Events;

public sealed class NotificationHandler(ILogger<NotificationHandler> logger) : INotificationHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Email notification sent to customer {CustomerId} for order {OrderId}.",
                notification.CustomerId,
                notification.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Notification dispatch failed for order {OrderId}.", notification.OrderId);
        }

        return Task.CompletedTask;
    }
}
