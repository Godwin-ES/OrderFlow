namespace OrderFlow.Domain.Interfaces;

using OrderFlow.Domain.Entities;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
