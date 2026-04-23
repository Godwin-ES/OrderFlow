namespace OrderFlow.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Interfaces;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Repositories;
using OrderFlow.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderFlowDb")
            ?? throw new InvalidOperationException("Connection string 'OrderFlowDb' was not found.");

        // Parse URI format (e.g., Render's postgres:// URL)
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            connectionString = $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={(userInfo.Length > 1 ? userInfo[1] : "")};";
        }

        services.AddDbContext<OrderFlowDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddHostedService<DataSeederService>();
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}
