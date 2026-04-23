namespace OrderFlow.Tests.Unit.Application;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderFlow.Application.Commands.PlaceOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Interfaces;

public sealed class PlaceOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnExistingOrder_WhenIdempotencyKeyExists()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var productRepository = new Mock<IProductRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<PlaceOrderCommandHandler>>();

        var existingOrder = Order.Create(Guid.NewGuid(), "idempotency-1");
        var product = Product.Create("Widget", "WID", 10m, 10);
        existingOrder.AddItem(product, 1);

        orderRepository.Setup(x => x.GetByIdempotencyKeyAsync("idempotency-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        productRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        var handler = new PlaceOrderCommandHandler(orderRepository.Object, productRepository.Object, outboxRepository.Object, unitOfWork.Object, logger.Object);

        var result = await handler.Handle(new PlaceOrderCommand(existingOrder.CustomerId, "idempotency-1", [new PlaceOrderItemCommand(product.Id, 1)]), CancellationToken.None);

        result.Order.OrderId.Should().Be(existingOrder.Id);
        result.IsIdempotentReplay.Should().BeTrue();
        outboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductMissing()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var productRepository = new Mock<IProductRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<PlaceOrderCommandHandler>>();

        orderRepository.Setup(x => x.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        productRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new PlaceOrderCommandHandler(orderRepository.Object, productRepository.Object, outboxRepository.Object, unitOfWork.Object, logger.Object);
        var command = new PlaceOrderCommand(Guid.NewGuid(), Guid.NewGuid().ToString(), [new PlaceOrderItemCommand(Guid.NewGuid(), 1)]);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldRetryOnConcurrencyException_ThenSucceed()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var productRepository = new Mock<IProductRepository>();
        var outboxRepository = new Mock<IOutboxRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<PlaceOrderCommandHandler>>();

        var product = Product.Create("Widget", "WID-2", 10m, 10);
        orderRepository.Setup(x => x.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        productRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        unitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException())
            .ReturnsAsync(1);

        var handler = new PlaceOrderCommandHandler(orderRepository.Object, productRepository.Object, outboxRepository.Object, unitOfWork.Object, logger.Object);
        var command = new PlaceOrderCommand(Guid.NewGuid(), Guid.NewGuid().ToString(), [new PlaceOrderItemCommand(product.Id, 1)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsIdempotentReplay.Should().BeFalse();
        result.Order.TotalAmount.Should().Be(10m);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
