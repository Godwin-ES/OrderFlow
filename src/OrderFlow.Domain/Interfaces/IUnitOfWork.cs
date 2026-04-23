namespace OrderFlow.Domain.Interfaces;

public interface IUnitOfWork
{
    void ClearTracking();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
