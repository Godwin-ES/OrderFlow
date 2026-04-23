namespace OrderFlow.Application.DTOs;

public sealed class PlaceOrderResult
{
    public required OrderResponse Order { get; init; }
    public required bool IsIdempotentReplay { get; init; }
}