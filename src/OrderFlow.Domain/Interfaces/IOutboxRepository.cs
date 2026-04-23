namespace OrderFlow.Domain.Interfaces;

using OrderFlow.Domain.Entities;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, CancellationToken cancellationToken = default);
}
