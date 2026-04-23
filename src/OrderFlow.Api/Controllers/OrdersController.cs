namespace OrderFlow.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Commands.PlaceOrder;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Queries.GetOrderById;
using OrderFlow.Application.Queries.ListOrders;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(
            request.CustomerId,
            request.IdempotencyKey,
            request.Items.Select(i => new PlaceOrderItemCommand(i.ProductId, i.Quantity)).ToList());

        var result = await mediator.Send(command, cancellationToken);
        if (result.IsIdempotentReplay)
        {
            return Ok(result.Order);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.OrderId }, result.Order);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<OrderResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new ListOrdersQuery(page, pageSize), cancellationToken);
        return Ok(response);
    }
}
