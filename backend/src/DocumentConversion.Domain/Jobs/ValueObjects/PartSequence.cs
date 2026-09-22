namespace DocumentConversion.Domain.Jobs.ValueObjects;

public readonly record struct PartSequence(int PartNumber, int TotalParts)
{
    public void EnsureValid()
    {
        if (PartNumber < 1 || TotalParts < 1 || PartNumber > TotalParts)
            throw new ArgumentOutOfRangeException(nameof(PartNumber), "Invalid part sequence.");
    }

    public string DisplayLabel => $"Part {PartNumber} of {TotalParts}";
}
