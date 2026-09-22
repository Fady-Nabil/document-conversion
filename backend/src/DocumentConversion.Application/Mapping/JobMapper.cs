using DocumentConversion.Application.Dtos;
using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Application.Mapping;

internal static class JobMapper
{
    public static JobSummaryDto ToSummary(ConversionJob job) => new(
        job.Id,
        job.Status.ToString(),
        job.OutputFormat,
        job.SourceFileName,
        job.CreatedAt,
        job.CompletedAt,
        job.ErrorCode?.ToString(),
        job.ErrorMessage);

    public static JobDetailDto ToDetail(ConversionJob job) => new(
        job.Id,
        job.Status.ToString(),
        job.OutputFormat,
        job.SourceFileName,
        job.CreatedAt,
        job.CompletedAt,
        job.ErrorCode?.ToString(),
        job.ErrorMessage,
        job.Events.Select(e => new JobEventDto(e.EventType.ToString(), e.Message, e.OccurredAt)).ToList(),
        job.Artifacts.Select(a =>
        {
            var seq = new PartSequence(a.PartNumber, a.TotalParts);
            return new OutputArtifactDto(a.PartNumber, a.TotalParts, seq.DisplayLabel, a.FileName, a.SizeBytes);
        }).ToList());
}
