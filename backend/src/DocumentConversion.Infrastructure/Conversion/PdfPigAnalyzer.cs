using DocumentConversion.Application.Abstractions;

namespace DocumentConversion.Infrastructure.Conversion;

internal sealed class PdfPigAnalyzer : IPdfAnalyzer
{
    public Task<PdfAnalysisResult> AnalyzeAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        using var document = PdfDocumentOpener.Open(pdfStream);
        var totalChars = 0;
        foreach (var page in document.GetPages())
            totalChars += page.Text?.Length ?? 0;

        return Task.FromResult(new PdfAnalysisResult
        {
            PageCount = document.NumberOfPages,
            TotalExtractableCharacters = totalChars
        });
    }
}
