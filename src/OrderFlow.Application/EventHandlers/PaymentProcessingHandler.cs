namespace OrderFlow.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Interfaces;

public sealed class PaymentProcessingHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentProcessingHandler> logger)
    : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(200, cancellationToken);

            var order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
            if (order is null)
            {
                logger.LogWarning("Order {OrderId} not found while processing payment.", notification.OrderId);
                return;
            }

            var shouldFail = notification.OrderId.ToByteArray()[0] % 10 == 0;
            if (shouldFail)
            {
                order.MarkAsFailed();
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogWarning("Payment simulation failed for order {OrderId}.", notification.OrderId);
                return;
            }

            order.MarkAsPaid();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Payment processed for order {OrderId}.", notification.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment processing failed unexpectedly for order {OrderId}.", notification.OrderId);
        }
    }
}
