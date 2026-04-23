namespace OrderFlow.Tests.Unit.Application;

using FluentValidation.TestHelper;
using OrderFlow.Application.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Items_Empty()
    {
        var command = new PlaceOrderCommand(Guid.NewGuid(), Guid.NewGuid().ToString(), []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_ProductIds()
    {
        var productId = Guid.NewGuid();
        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            [new PlaceOrderItemCommand(productId, 1), new PlaceOrderItemCommand(productId, 1)]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }
}
