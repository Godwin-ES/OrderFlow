namespace OrderFlow.Application.DTOs;

public sealed class PlaceOrderRequest
{
    public Guid CustomerId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public IReadOnlyList<PlaceOrderItemRequest> Items { get; set; } = [];
}

public sealed class PlaceOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
