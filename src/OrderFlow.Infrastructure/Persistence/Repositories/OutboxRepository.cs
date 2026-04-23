namespace OrderFlow.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Interfaces;

public sealed class OutboxRepository(OrderFlowDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        => await dbContext.OutboxMessages.AddAsync(message, cancellationToken);

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, CancellationToken cancellationToken = default)
        => await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.RetryCount < 5)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
