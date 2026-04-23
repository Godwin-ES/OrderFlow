namespace OrderFlow.Application.Queries.ListOrders;

using MediatR;
using OrderFlow.Application.DTOs;

public sealed record ListOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedResult<OrderResponse>>;
