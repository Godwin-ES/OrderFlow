namespace OrderFlow.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(string eventType, string payload)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }

    public static OutboxMessage Create(string eventType, string payload) => new(eventType, payload);

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailedAttempt()
    {
        RetryCount += 1;
    }
}
