namespace OrderFlow.Domain.Events;

using MediatR;

public sealed record OrderPlacedItem(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    IReadOnlyList<OrderPlacedItem> Items) : INotification;
