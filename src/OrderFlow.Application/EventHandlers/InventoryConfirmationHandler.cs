namespace OrderFlow.Application.EventHandlers;

using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Interfaces;

public sealed class InventoryConfirmationHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<InventoryConfirmationHandler> logger)
    : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
            if (order is null)
            {
                return;
            }

            if (order.Status == OrderStatus.PaymentProcessed)
            {
                order.MarkAsConfirmed();
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Inventory deduction confirmed for order {OrderId}.", notification.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Inventory confirmation failed for order {OrderId}.", notification.OrderId);
        }
    }
}
