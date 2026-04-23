namespace OrderFlow.Application.Queries.GetOrderById;

using MediatR;
using OrderFlow.Application.DTOs;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponse>;
