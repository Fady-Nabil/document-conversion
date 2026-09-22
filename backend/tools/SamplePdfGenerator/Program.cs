var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "samples"));
Directory.CreateDirectory(root);

File.WriteAllBytes(Path.Combine(root, "text-based.pdf"), BuildPdf(SamplePageContents.TextBasedPageContent));
File.WriteAllBytes(Path.Combine(root, "mixed-text-and-scan-pages.pdf"), BuildPdf(SamplePageContents.MixedTextPageContent));
File.WriteAllBytes(Path.Combine(root, "scanned-image-only.pdf"), BuildPdf(Array.Empty<string>()));
File.WriteAllBytes(Path.Combine(root, "large-multi-page.pdf"), BuildLargePdf(60, 5000));

Console.WriteLine($"Samples written to {root}");

static byte[] BuildLargePdf(int pages, int charsPerPage)
{
    var streams = new string[pages];
    var chunk = new string('X', charsPerPage);
    for (var i = 0; i < pages; i++)
    {
        var escaped = $"Page {i + 1} {chunk}".Replace("(", "\\(").Replace(")", "\\)");
        streams[i] = $"BT /F1 10 Tf 72 720 Td ({escaped}) Tj ET";
    }
    return BuildPdf(streams);
}

static byte[] BuildPdf(string[] pageContents)
{
    using var ms = new MemoryStream();
    using var writer = new StreamWriter(ms);
    writer.WriteLine("%PDF-1.4");
    var offsets = new List<long> { 0 };

    void WriteObject(int id, string body)
    {
        offsets.Add(ms.Position);
        writer.WriteLine($"{id} 0 obj");
        writer.WriteLine(body);
        writer.WriteLine("endobj");
        writer.Flush();
    }

    var kids = new List<int>();
    var pageCount = Math.Max(1, pageContents.Length);
    var firstContentId = 3;
    var fontId = firstContentId + pageCount * 2;

    for (var i = 0; i < pageCount; i++)
    {
        var contentId = firstContentId + i * 2;
        var pageId = contentId + 1;
        kids.Add(pageId);
        var content = pageContents.Length > i ? pageContents[i] : string.Empty;
        WriteObject(contentId, $"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        WriteObject(pageId,
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents {contentId} 0 R /Resources << /Font << /F1 {fontId} 0 R >> >> >>");
    }

    WriteObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
    WriteObject(2, $"<< /Type /Pages /Kids [{string.Join(" ", kids.Select(k => $"{k} 0 R"))}] /Count {kids.Count} >>");
    WriteObject(fontId, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

    var xrefPos = ms.Position;
    writer.WriteLine("xref");
    writer.WriteLine($"0 {offsets.Count}");
    writer.WriteLine("0000000000 65535 f ");
    foreach (var off in offsets.Skip(1))
        writer.WriteLine($"{off:D10} 00000 n ");
    writer.WriteLine("trailer");
    writer.WriteLine($"<< /Size {offsets.Count} /Root 1 0 R >>");
    writer.WriteLine("startxref");
    writer.WriteLine(xrefPos);
    writer.WriteLine("%%EOF");
    writer.Flush();
    return ms.ToArray();
}

file static class SamplePageContents
{
    internal static readonly string[] TextBasedPageContent =
    [
        "BT /F1 12 Tf 72 720 Td (Digital Arena sample document with extractable text.) Tj ET"
    ];

    internal static readonly string[] MixedTextPageContent =
    [
        "BT /F1 12 Tf 72 720 Td (Page one contains searchable text.) Tj ET"
    ];
}
