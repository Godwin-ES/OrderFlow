namespace OrderFlow.Domain.Entities;

using OrderFlow.Domain.Enums;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    private Order(Guid id, Guid customerId, string idempotencyKey)
    {
        Id = id;
        CustomerId = customerId;
        IdempotencyKey = idempotencyKey;
        Status = OrderStatus.Placed;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

    public static Order Create(Guid customerId, string idempotencyKey)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        return new Order(Guid.NewGuid(), customerId, idempotencyKey.Trim());
    }

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        _items.Add(new OrderItem(Id, product.Id, quantity, product.UnitPrice));
        RecalculateTotal();
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.PaymentProcessed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsConfirmed()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = OrderStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
        UpdatedAt = DateTime.UtcNow;
    }
}
