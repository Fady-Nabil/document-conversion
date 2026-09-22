namespace DocumentConversion.Application.Dtos;

public sealed record JobSummaryDto(
    Guid Id,
    string Status,
    string OutputFormat,
    string SourceFileName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record JobEventDto(string EventType, string Message, DateTimeOffset OccurredAt);

public sealed record OutputArtifactDto(
    int PartNumber,
    int TotalParts,
    string DisplayLabel,
    string FileName,
    long SizeBytes);

public sealed record JobDetailDto(
    Guid Id,
    string Status,
    string OutputFormat,
    string SourceFileName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorCode,
    string? ErrorMessage,
    IReadOnlyList<JobEventDto> Events,
    IReadOnlyList<OutputArtifactDto> Artifacts);

public sealed record PagedJobsDto(IReadOnlyList<JobSummaryDto> Items, int TotalCount);
