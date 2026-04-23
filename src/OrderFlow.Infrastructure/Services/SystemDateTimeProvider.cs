namespace OrderFlow.Infrastructure.Services;

using OrderFlow.Application.Interfaces;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
