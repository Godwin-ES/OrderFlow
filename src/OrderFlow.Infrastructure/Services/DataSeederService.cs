namespace OrderFlow.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

public sealed class DataSeederService(IServiceScopeFactory serviceScopeFactory, ILogger<DataSeederService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var products = new[]
        {
            Product.Create("Widget A", "WIDGET-A", 19.99m, 100),
            Product.Create("Widget B", "WIDGET-B", 20.00m, 100),
            Product.Create("Widget C", "WIDGET-C", 9.50m, 250)
        };

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded default products.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
