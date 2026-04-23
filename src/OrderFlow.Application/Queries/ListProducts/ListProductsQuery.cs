namespace OrderFlow.Application.Queries.ListProducts;

using MediatR;
using OrderFlow.Application.DTOs;

public sealed record ListProductsQuery : IRequest<IReadOnlyList<ProductResponse>>;
