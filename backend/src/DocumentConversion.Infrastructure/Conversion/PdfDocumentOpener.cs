using DocumentConversion.Domain.Jobs.Exceptions;
using UglyToad.PdfPig;

namespace DocumentConversion.Infrastructure.Conversion;

internal static class PdfDocumentOpener
{
    public static PdfDocument Open(Stream pdfStream)
    {
        try
        {
            if (pdfStream.CanSeek)
                pdfStream.Position = 0;

            using var buffer = new MemoryStream();
            pdfStream.CopyTo(buffer);

            if (pdfStream.CanSeek)
                pdfStream.Position = 0;

            return PdfDocument.Open(buffer.ToArray());
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOrCorruptedInputException("Unable to read PDF input.", ex);
        }
    }
}
