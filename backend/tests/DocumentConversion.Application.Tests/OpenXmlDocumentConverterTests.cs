using DocumentConversion.Infrastructure.Conversion;
using FluentAssertions;

namespace DocumentConversion.Application.Tests;

public class OpenXmlDocumentConverterTests
{
    [Fact]
    public async Task ConvertPdfToDocumentModel_CarriesTextToDocx()
    {
        var pdfBytes = MinimalPdfWithText("Assessment sample text");
        var converter = new OpenXmlDocumentConverter();

        using var stream = new MemoryStream(pdfBytes);
        var model = await converter.ConvertPdfToDocumentModelAsync(stream);

        model.Pages.Should().NotBeEmpty();
        model.TotalExtractableCharacters.Should().BeGreaterThan(0);
        model.Pages[0].SerializedDocx.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeThenConvert_OnSamePdfBytes_Succeeds()
    {
        var pdfBytes = MinimalPdfWithText("Assessment sample text");
        var analyzer = new PdfPigAnalyzer();
        var converter = new OpenXmlDocumentConverter();

        var analysis = await analyzer.AnalyzeAsync(new MemoryStream(pdfBytes), CancellationToken.None);
        var model = await converter.ConvertPdfToDocumentModelAsync(new MemoryStream(pdfBytes), CancellationToken.None);

        analysis.TotalExtractableCharacters.Should().BeGreaterThan(0);
        model.TotalExtractableCharacters.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Analyze_SampleLargeMultiPagePdf_Succeeds()
    {
        var samplePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "samples",
            "large-multi-page.pdf"));
        File.Exists(samplePath).Should().BeTrue("regenerate samples with SamplePdfGenerator if missing");

        var pdfBytes = await File.ReadAllBytesAsync(samplePath);
        var analyzer = new PdfPigAnalyzer();

        var analysis = await analyzer.AnalyzeAsync(new MemoryStream(pdfBytes), CancellationToken.None);

        analysis.PageCount.Should().BeGreaterThan(1);
        analysis.TotalExtractableCharacters.Should().BeGreaterThan(0);
    }

    private static byte[] MinimalPdfWithText(string text)
    {
        var escaped = text.Replace("(", "\\(").Replace(")", "\\)");
        var content = $"BT /F1 12 Tf 72 720 Td ({escaped}) Tj ET";
        var streamContent = $@"<< /Length {content.Length} >>
stream
{content}
endstream";

        var pdf = $"""
            %PDF-1.4
            1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj
            2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj
            3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj
            4 0 obj {streamContent} endobj
            5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj
            xref
            0 6
            0000000000 65535 f 
            0000000009 00000 n 
            0000000058 00000 n 
            0000000115 00000 n 
            0000000266 00000 n 
            0000000400 00000 n 
            trailer << /Size 6 /Root 1 0 R >>
            startxref
            480
            %%EOF
            """;

        return System.Text.Encoding.ASCII.GetBytes(pdf);
    }
}
