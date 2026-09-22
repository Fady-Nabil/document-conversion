using DocumentConversion.Application.Abstractions;
using DocumentConversion.Domain.Jobs.Exceptions;
using DocumentConversion.Domain.Jobs.Models;
using UglyToad.PdfPig.Content;

namespace DocumentConversion.Infrastructure.Conversion;

internal sealed class OpenXmlDocumentConverter : IDocumentConverter
{
    public Task<DocumentModel> ConvertPdfToDocumentModelAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        using var document = PdfDocumentOpener.Open(pdfStream);
        var pages = new List<PageModel>();

        foreach (var page in document.GetPages())
        {
            var text = page.Text ?? string.Empty;
            var images = ExtractImages(page);
            var pageDocx = DocxBuilder.CreateFromContent(text, images);
            pages.Add(new PageModel
            {
                PageIndex = page.Number,
                Text = text,
                Images = images,
                SerializedDocx = pageDocx
            });
        }

        return Task.FromResult(new DocumentModel { Pages = pages });
    }

    public byte[] BuildPartDocx(DocumentModel document, IReadOnlyList<int> pageIndices)
    {
        var pages = document.Pages.Where(p => pageIndices.Contains(p.PageIndex)).OrderBy(p => p.PageIndex).ToList();
        if (pages.Count is 0)
            throw new EmptyDocumentException("No pages selected for output part.");

        if (pages.Count is 1)
            return pages[0].SerializedDocx;

        var combinedText = string.Join(Environment.NewLine + Environment.NewLine,
            pages.Select(p => p.Text));
        var combinedImages = pages.SelectMany(p => p.Images).ToList();
        return DocxBuilder.CreateFromContent(combinedText, combinedImages, pages.First().PageIndex, pages.Last().PageIndex);
    }

    private static List<byte[]> ExtractImages(Page page)
    {
        var images = new List<byte[]>();
        foreach (var image in page.GetImages())
        {
            var bytes = image.RawBytes.ToArray();
            if (bytes.Length > 0)
                images.Add(bytes);
        }

        return images;
    }
}
