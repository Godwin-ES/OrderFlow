namespace OrderFlow.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Interfaces;

public sealed class ProductRepository(OrderFlowDbContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var uniqueIds = ids.Distinct().ToArray();
        return await dbContext.Products.Where(x => uniqueIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await dbContext.Products.AddAsync(product, cancellationToken);

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default)
        => await dbContext.Products.OrderBy(x => x.Name).ToListAsync(cancellationToken);
}
