namespace OrderFlow.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.DTOs;
using OrderFlow.Tests.Helpers;

public sealed class OrdersApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PlaceOrder_ShouldBeIdempotent_ForSameKey()
    {
        var product = await CreateProductAsync();

        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Items = [new PlaceOrderItemRequest { ProductId = product.Id, Quantity = 1 }]
        };

        var first = await _client.PostAsJsonAsync("/api/orders", request);
        var second = await _client.PostAsJsonAsync("/api/orders", request);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstOrder = await first.Content.ReadFromJsonAsync<OrderResponse>();
        var secondOrder = await second.Content.ReadFromJsonAsync<OrderResponse>();

        firstOrder.Should().NotBeNull();
        secondOrder.Should().NotBeNull();
        secondOrder!.OrderId.Should().Be(firstOrder!.OrderId);
    }

    private async Task<ProductResponse> CreateProductAsync()
    {
        var payload = new
        {
            name = "Integration Widget",
            sku = $"SKU-{Guid.NewGuid():N}",
            unitPrice = 20.0m,
            stockQuantity = 100
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!;
    }
}
