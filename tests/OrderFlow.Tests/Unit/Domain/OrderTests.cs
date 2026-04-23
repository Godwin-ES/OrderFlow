namespace OrderFlow.Tests.Unit.Domain;

using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

public sealed class OrderTests
{
    [Fact]
    public void AddItem_ShouldUpdateTotalAmount()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid().ToString());
        var productA = Product.Create("Widget A", "A", 19.99m, 10);
        var productB = Product.Create("Widget B", "B", 5.00m, 10);

        order.AddItem(productA, 2);
        order.AddItem(productB, 1);

        order.TotalAmount.Should().Be(44.98m);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void MarkAsPaid_ShouldSetStatus()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid().ToString());

        order.MarkAsPaid();

        order.Status.Should().Be(OrderStatus.PaymentProcessed);
    }
}
