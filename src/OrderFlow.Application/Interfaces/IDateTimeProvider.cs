namespace OrderFlow.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
