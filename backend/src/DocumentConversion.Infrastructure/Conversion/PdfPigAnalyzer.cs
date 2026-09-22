using DocumentConversion.Application.Abstractions;
using DocumentConversion.Domain.Jobs.Exceptions;

namespace DocumentConversion.Infrastructure.Conversion;

internal sealed class PdfPigAnalyzer : IPdfAnalyzer
{
    public Task<PdfAnalysisResult> AnalyzeAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = PdfDocumentOpener.Open(pdfStream);
            var totalChars = 0;
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                totalChars += page.Text?.Length ?? 0;
            }

            return Task.FromResult(new PdfAnalysisResult
            {
                PageCount = document.NumberOfPages,
                TotalExtractableCharacters = totalChars
            });
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOrCorruptedInputException("Unable to analyze PDF input.", ex);
        }
    }
}
