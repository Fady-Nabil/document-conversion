using BuildingBlocks.Domain.Abstractions;
using DocumentConversion.Domain.Jobs.Entities;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Domain.Jobs.Aggregates;

public sealed class ConversionJob : AggregateRoot
{
    private readonly List<JobEvent> _events = [];
    private readonly List<OutputArtifact> _artifacts = [];

    public JobStatus Status { get; private set; }
    public string OutputFormat { get; private set; } = string.Empty;
    public string SourceFileName { get; private set; } = string.Empty;
    public string SourceRelativePath { get; private set; } = string.Empty;
    public DomainErrorCode? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ContentFingerprint { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public IReadOnlyList<JobEvent> Events => _events.AsReadOnly();
    public IReadOnlyList<OutputArtifact> Artifacts => _artifacts.AsReadOnly();

    private ConversionJob() { }

    public static ConversionJob Submit(JobId id, OutputFormat format, string sourceFileName, string sourceRelativePath)
    {
        var job = new ConversionJob
        {
            Id = id.Value,
            Status = JobStatus.Submitted,
            OutputFormat = format.Value,
            SourceFileName = sourceFileName,
            SourceRelativePath = sourceRelativePath,
            CreatedAt = DateTimeOffset.UtcNow
        };
        job._events.Add(JobEvent.Create(job.Id, JobEventType.Submitted, "Job submitted."));
        return job;
    }

    public bool IsTerminal => Status is JobStatus.Completed or JobStatus.Failed or JobStatus.NeedsReview;

    public void MarkProcessing()
    {
        EnsureNotTerminal();
        Status = JobStatus.Processing;
        _events.Add(JobEvent.Create(Id, JobEventType.Started, "Processing started."));
    }

    public void RecordConversionSucceeded(ContentFingerprint fingerprint)
    {
        ContentFingerprint = fingerprint.Value;
        _events.Add(JobEvent.Create(Id, JobEventType.ConversionSucceeded, "Conversion succeeded."));
    }

    public void RecordSplitIntoParts(int totalParts)
    {
        _events.Add(JobEvent.Create(Id, JobEventType.SplitIntoParts, $"Split into {totalParts} part(s)."));
    }

    public void AttachArtifacts(IEnumerable<OutputArtifact> artifacts)
    {
        _artifacts.Clear();
        _artifacts.AddRange(artifacts);
    }

    public void Complete()
    {
        if (_artifacts.Count is 0)
            throw new InvalidOperationException("Cannot complete without artifacts.");

        Status = JobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        _events.Add(JobEvent.Create(Id, JobEventType.ValidationPassed, "Output validation passed."));
        _events.Add(JobEvent.Create(Id, JobEventType.Completed, "Job completed successfully."));
    }

    public void Fail(DomainErrorCode code, string message)
    {
        if (Status is JobStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed job.");

        Status = JobStatus.Failed;
        ErrorCode = code;
        ErrorMessage = message;
        CompletedAt = DateTimeOffset.UtcNow;
        _events.Add(JobEvent.Create(Id, JobEventType.Failed, $"{code}: {message}"));
    }

    public void MarkNeedsReview(string message)
    {
        if (Status is JobStatus.Completed)
            throw new InvalidOperationException("Cannot mark completed job for review.");

        Status = JobStatus.NeedsReview;
        ErrorCode = DomainErrorCode.ValidationFailed;
        ErrorMessage = message;
        CompletedAt = DateTimeOffset.UtcNow;
        _events.Add(JobEvent.Create(Id, JobEventType.ValidationFailed, message));
        _events.Add(JobEvent.Create(Id, JobEventType.NeedsReview, "Job flagged for review."));
    }

    private void EnsureNotTerminal()
    {
        if (IsTerminal)
            throw new InvalidOperationException($"Job is already terminal ({Status}).");
    }
}
