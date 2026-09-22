using BuildingBlocks.Domain.Abstractions;
using DocumentConversion.Domain.Jobs.Enums;

namespace DocumentConversion.Domain.Jobs.Entities;

public sealed class JobEvent : Entity
{
    public Guid JobId { get; private set; }
    public JobEventType EventType { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }

    private JobEvent() { }

    public static JobEvent Create(Guid jobId, JobEventType eventType, string message)
    {
        return new JobEvent
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            EventType = eventType,
            Message = message,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }
}
