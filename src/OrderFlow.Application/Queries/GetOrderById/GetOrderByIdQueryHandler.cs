namespace OrderFlow.Application.Queries.GetOrderById;

using MediatR;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Interfaces;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository, IProductRepository productRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

        var products = await productRepository.GetByIdsAsync(order.Items.Select(i => i.ProductId).Distinct(), cancellationToken);
        var names = products.ToDictionary(p => p.Id, p => p.Name);

        return new OrderResponse
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
        };
    }
}
