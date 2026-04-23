namespace OrderFlow.Api.Middleware;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Exceptions;

public sealed class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            ProductNotFoundException => (StatusCodes.Status404NotFound, "Product not found"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DuplicateOrderException => (StatusCodes.Status409Conflict, "Duplicate order"),
            InsufficientStockException => (StatusCodes.Status409Conflict, "Insufficient stock"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var details = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = exception.Message,
            Type = $"https://httpstatuses.io/{status}"
        };

        await context.Response.WriteAsJsonAsync(details);
    }
}
