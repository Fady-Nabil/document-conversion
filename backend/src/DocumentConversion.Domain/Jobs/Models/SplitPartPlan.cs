namespace DocumentConversion.Domain.Jobs.Models;

public sealed class SplitPartPlan
{
    public int PartNumber { get; init; }
    public int TotalParts { get; init; }
    public IReadOnlyList<int> PageIndices { get; init; } = [];
    public byte[] DocxContent { get; set; } = [];
    public long SizeBytes => DocxContent.LongLength;
}
