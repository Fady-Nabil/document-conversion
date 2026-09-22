using BuildingBlocks.Domain.Abstractions;
using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Domain.Jobs.Entities;

public sealed class OutputArtifact : Entity
{
    public Guid JobId { get; private set; }
    public int PartNumber { get; private set; }
    public int TotalParts { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string RelativePath { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; } = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    private OutputArtifact() { }

    public static OutputArtifact Create(Guid jobId, PartSequence sequence, string fileName, string relativePath, long sizeBytes)
    {
        sequence.EnsureValid();
        return new OutputArtifact
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            PartNumber = sequence.PartNumber,
            TotalParts = sequence.TotalParts,
            FileName = fileName,
            RelativePath = relativePath,
            SizeBytes = sizeBytes
        };
    }
}
