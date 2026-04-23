namespace OrderFlow.Tests.Unit.Domain;

using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Exceptions;

public sealed class ProductTests
{
    [Fact]
    public void DeductStock_ShouldReduceStock_WhenQuantityAvailable()
    {
        var product = Product.Create("Widget", "WIDGET", 10m, 10);

        product.DeductStock(3);

        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void DeductStock_ShouldThrow_WhenQuantityExceedsStock()
    {
        var product = Product.Create("Widget", "WIDGET", 10m, 2);

        var act = () => product.DeductStock(3);

        act.Should().Throw<InsufficientStockException>();
    }
}
