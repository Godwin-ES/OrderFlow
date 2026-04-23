namespace OrderFlow.Application.Commands.PlaceOrder;

using MediatR;
using OrderFlow.Application.DTOs;

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    string IdempotencyKey,
    IReadOnlyList<PlaceOrderItemCommand> Items) : IRequest<PlaceOrderResult>;

public sealed record PlaceOrderItemCommand(Guid ProductId, int Quantity);
