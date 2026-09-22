namespace DocumentConversion.Domain.Jobs.Models;

public sealed class StoredPartInfo
{
    public int PartNumber { get; init; }
    public int TotalParts { get; init; }
    public IReadOnlyList<int> PageIndices { get; init; } = Array.Empty<int>();
    public long SizeBytes { get; init; }
}
