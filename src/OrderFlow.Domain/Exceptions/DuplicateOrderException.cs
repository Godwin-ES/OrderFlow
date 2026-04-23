namespace OrderFlow.Domain.Exceptions;

public sealed class DuplicateOrderException : Exception
{
    public DuplicateOrderException(string idempotencyKey)
        : base($"An order already exists for idempotency key '{idempotencyKey}'.")
    {
    }
}
