using DocumentConversion.Domain.Jobs.Models;

namespace DocumentConversion.Application.Abstractions;

public interface IDocumentConverter
{
    Task<DocumentModel> ConvertPdfToDocumentModelAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    byte[] BuildPartDocx(DocumentModel document, IReadOnlyList<int> pageIndices);
}
