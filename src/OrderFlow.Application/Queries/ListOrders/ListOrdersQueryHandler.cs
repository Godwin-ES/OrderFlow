namespace OrderFlow.Application.Queries.ListOrders;

using MediatR;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Interfaces;

public sealed class ListOrdersQueryHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    : IRequestHandler<ListOrdersQuery, PaginatedResult<OrderResponse>>
{
    public async Task<PaginatedResult<OrderResponse>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var orders = await orderRepository.ListAsync(page, pageSize + 1, cancellationToken);
        var pagedOrders = orders.Take(pageSize).ToList();

        var productIds = pagedOrders.SelectMany(o => o.Items.Select(i => i.ProductId)).Distinct().ToArray();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var names = products.ToDictionary(p => p.Id, p => p.Name);

        return new PaginatedResult<OrderResponse>
        {
            Items = pagedOrders.Select(order => new OrderResponse
            {
                OrderId = order.Id,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = names.GetValueOrDefault(i.ProductId, "Unknown"),
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            HasNextPage = orders.Count > pageSize
        };
    }
}
