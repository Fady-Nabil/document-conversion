using DocumentConversion.Domain.Jobs.Exceptions;
using DocumentConversion.Domain.Jobs.Models;

namespace DocumentConversion.Domain.Jobs.Services;

public sealed class DocumentSplittingService
{
    public static IReadOnlyList<SplitPartPlan> PlanParts(DocumentModel document, long maxPartSizeBytes)
    {
        if (document.IsEmpty)
            throw new EmptyDocumentException("Document has no convertible content.");

        var pages = document.Pages.OrderBy(p => p.PageIndex).ToList();
        foreach (var page in pages)
        {
            if (page.SerializedDocx.LongLength > maxPartSizeBytes)
            {
                throw new UnsplittableContentExceedsLimitException(
                    $"Page {page.PageIndex} serializes to {page.SerializedDocx.LongLength} bytes, exceeding the {maxPartSizeBytes} byte limit.");
            }
        }

        var groups = new List<List<PageModel>>();
        var current = new List<PageModel>();
        long currentSize = 0;

        foreach (var page in pages)
        {
            var pageSize = page.SerializedDocx.LongLength;
            if (current.Count > 0 && currentSize + pageSize > maxPartSizeBytes)
            {
                groups.Add(current);
                current = [];
                currentSize = 0;
            }

            current.Add(page);
            currentSize += pageSize;
        }

        if (current.Count > 0)
            groups.Add(current);

        var totalParts = groups.Count;
        var plans = new List<SplitPartPlan>();

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            plans.Add(new SplitPartPlan
            {
                PartNumber = i + 1,
                TotalParts = totalParts,
                PageIndices = [.. group.Select(p => p.PageIndex)],
                DocxContent = []
            });
        }

        return plans;
    }
}
