namespace OrderFlow.Domain.Exceptions;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Insufficient stock for product '{productId}'. Requested: {requested}, available: {available}.")
    {
    }
}
