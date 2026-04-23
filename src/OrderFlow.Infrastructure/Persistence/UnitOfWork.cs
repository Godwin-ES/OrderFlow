namespace OrderFlow.Infrastructure.Persistence;

using OrderFlow.Domain.Interfaces;

public sealed class UnitOfWork(OrderFlowDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
