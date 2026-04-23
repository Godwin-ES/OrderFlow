namespace OrderFlow.Application.Commands.PlaceOrder;

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Interfaces;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork,
    ILogger<PlaceOrderCommandHandler> logger)
    : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private const int MaxRetries = 3;

    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await orderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingOrder is not null)
        {
            return new PlaceOrderResult
            {
                Order = await ToOrderResponseAsync(existingOrder, cancellationToken),
                IsIdempotentReplay = true
            };
        }

        var attempt = 0;

        while (true)
        {
            try
            {
                var productIds = request.Items.Select(i => i.ProductId).Distinct().ToArray();
                var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
                var productMap = products.ToDictionary(p => p.Id);

                foreach (var productId in productIds)
                {
                    if (!productMap.ContainsKey(productId))
                    {
                        throw new ProductNotFoundException(productId);
                    }
                }

                var order = Order.Create(request.CustomerId, request.IdempotencyKey);

                foreach (var item in request.Items)
                {
                    var product = productMap[item.ProductId];
                    product.DeductStock(item.Quantity);
                    order.AddItem(product, item.Quantity);
                }

                await orderRepository.AddAsync(order, cancellationToken);

                var orderPlacedEvent = new OrderPlacedEvent(
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    order.Items.Select(i => new OrderPlacedItem(i.ProductId, productMap[i.ProductId].Name, i.Quantity, i.UnitPrice)).ToList());

                var outboxPayload = JsonSerializer.Serialize(orderPlacedEvent);
                await outboxRepository.AddAsync(OutboxMessage.Create(nameof(OrderPlacedEvent), outboxPayload), cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return new PlaceOrderResult
                {
                    Order = await ToOrderResponseAsync(order, cancellationToken),
                    IsIdempotentReplay = false
                };
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
            {
                attempt++;
                logger.LogWarning("Concurrency conflict while placing order with idempotency key {IdempotencyKey}. Retry {Attempt}/{MaxRetries}.", request.IdempotencyKey, attempt, MaxRetries);
            }
            catch (DbUpdateException)
            {
                var duplicate = await orderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
                if (duplicate is not null)
                {
                    return new PlaceOrderResult
                    {
                        Order = await ToOrderResponseAsync(duplicate, cancellationToken),
                        IsIdempotentReplay = true
                    };
                }

                throw;
            }
        }
    }

    private async Task<OrderResponse> ToOrderResponseAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(i => i.ProductId).Distinct();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var names = products.ToDictionary(p => p.Id, p => p.Name);

        return new OrderResponse
        {
            OrderId = order.Id,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items
                .Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = names.GetValueOrDefault(i.ProductId, "Unknown"),
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };
    }
}
