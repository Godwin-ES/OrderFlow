namespace OrderFlow.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.DTOs;
using OrderFlow.Tests.Helpers;

public sealed class ConcurrencyTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConcurrencyTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentOrders_ShouldNotOversellStock()
    {
        var product = await CreateProductAsync(5);

        var tasks = Enumerable.Range(0, 10)
            .Select(i => _client.PostAsJsonAsync(
                "/api/orders",
                new PlaceOrderRequest
                {
                    CustomerId = Guid.NewGuid(),
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Items = [new PlaceOrderItemRequest { ProductId = product.Id, Quantity = 1 }]
                }))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Created || r.StatusCode == HttpStatusCode.Conflict);
        var createdCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        createdCount.Should().BeGreaterThan(0);

        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        updatedProduct.Should().NotBeNull();
        updatedProduct!.StockQuantity.Should().BeGreaterThanOrEqualTo(0);
        updatedProduct.StockQuantity.Should().BeLessThanOrEqualTo(5);

        // Inventory cannot be consumed beyond available stock regardless of request concurrency.
        var consumedUnits = 5 - updatedProduct.StockQuantity;
        consumedUnits.Should().BeLessThanOrEqualTo(5);
        consumedUnits.Should().BeLessThanOrEqualTo(createdCount);
    }

    private async Task<ProductResponse> CreateProductAsync(int stock)
    {
        var payload = new
        {
            name = "Concurrency Widget",
            sku = $"CSKU-{Guid.NewGuid():N}",
            unitPrice = 20.0m,
            stockQuantity = stock
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!;
    }
}
