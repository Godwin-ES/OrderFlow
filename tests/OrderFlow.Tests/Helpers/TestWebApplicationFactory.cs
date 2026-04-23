namespace OrderFlow.Tests.Helpers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Services;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<OrderFlowDbContext>));
            services.RemoveAll<OrderFlowDbContext>();

            var hostedServices = services
                .Where(sd => sd.ServiceType == typeof(IHostedService)
                    && (sd.ImplementationType == typeof(DataSeederService)
                        || sd.ImplementationType == typeof(OutboxProcessorService)))
                .ToList();

            foreach (var descriptor in hostedServices)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<OrderFlowDbContext>(options =>
            {
                options.UseInMemoryDatabase("OrderFlowTests", DbRoot);
            });
        });
    }
}
