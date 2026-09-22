namespace DocumentConversion.Application.Abstractions;

public interface IPdfAnalyzer
{
    Task<PdfAnalysisResult> AnalyzeAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}

public sealed class PdfAnalysisResult
{
    public int PageCount { get; init; }
    public int TotalExtractableCharacters { get; init; }
}
