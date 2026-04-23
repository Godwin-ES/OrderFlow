namespace OrderFlow.Domain.Exceptions;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product '{productId}' was not found.")
    {
    }
}
