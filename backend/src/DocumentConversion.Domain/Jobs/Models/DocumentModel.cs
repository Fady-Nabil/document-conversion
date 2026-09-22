namespace DocumentConversion.Domain.Jobs.Models;

public sealed class DocumentModel
{
    public IReadOnlyList<PageModel> Pages { get; init; } = [];

    public int TotalExtractableCharacters => Pages.Sum(p => p.TextLength);

    public bool IsEmpty => Pages.Count is 0 ||
                           Pages.All(p => p.TextLength is 0 && p.ImageBytesSum is 0);
}
