namespace OrderFlow.Domain.Entities;

using OrderFlow.Domain.Exceptions;

public sealed class Product
{
    private Product()
    {
    }

    private Product(Guid id, string name, string sku, decimal unitPrice, int stockQuantity)
    {
        Id = id;
        Name = name;
        Sku = sku;
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Product Create(string name, string sku, decimal unitPrice, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required.", nameof(sku));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity cannot be negative.");
        }

        return new Product(Guid.NewGuid(), name.Trim(), sku.Trim(), unitPrice, stockQuantity);
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (StockQuantity < quantity)
        {
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
