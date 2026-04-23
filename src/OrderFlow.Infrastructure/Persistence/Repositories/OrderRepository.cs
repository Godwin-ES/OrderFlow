namespace OrderFlow.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Interfaces;

public sealed class OrderRepository(OrderFlowDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AddAsync(order, cancellationToken);

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        => await dbContext.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(0, (page - 1) * pageSize);
        return await dbContext.Orders
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
