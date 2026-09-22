namespace DocumentConversion.Domain.Jobs.Models;

public sealed class PageModel
{
    public int PageIndex { get; init; }
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<byte[]> Images { get; init; } = [];
    public byte[] SerializedDocx { get; init; } = [];
    public int TextLength => Text.Length;
    public long ImageBytesSum => Images.Sum(i => (long)i.Length);
}
