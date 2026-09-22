namespace DocumentConversion.Domain.Jobs.ValueObjects;

public readonly record struct ContentFingerprint(string Value)
{
    public static ContentFingerprint FromPageMetrics(IReadOnlyList<PageFingerprintEntry> pages)
    {
        var parts = pages.OrderBy(p => p.PageIndex).Select(p => $"{p.PageIndex}:{p.TextLength}:{p.ImageBytesSum}");
        var raw = string.Join("|", parts);
        return new ContentFingerprint(raw);
    }
}

public readonly record struct PageFingerprintEntry(int PageIndex, int TextLength, long ImageBytesSum);
