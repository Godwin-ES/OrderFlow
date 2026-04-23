namespace OrderFlow.Application.DTOs;

using OrderFlow.Domain.Enums;

public sealed class OrderResponse
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<OrderItemResponse> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public sealed class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
