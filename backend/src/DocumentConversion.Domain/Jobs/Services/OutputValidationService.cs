using DocumentConversion.Domain.Jobs.Models;
using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Domain.Jobs.Services;

public sealed class OutputValidationService
{
    public static OutputValidationResult Validate(DocumentModel sourceDocument,
        ContentFingerprint preSplitFingerprint, IReadOnlyList<StoredPartInfo> storedParts, long maxPartSizeBytes)
    {
        var errors = new List<string>();
        var expectedPages = sourceDocument.Pages.Select(p => p.PageIndex).OrderBy(i => i).ToList();

        if (storedParts.Count is 0)
        {
            errors.Add("No output parts were produced.");
            return OutputValidationResult.Failure(errors.ToArray());
        }

        var totalParts = storedParts[0].TotalParts;
        if (storedParts.Any(p => p.TotalParts != totalParts))
            errors.Add("Inconsistent TotalParts across artifacts.");

        var partNumbers = storedParts.Select(p => p.PartNumber).OrderBy(n => n).ToList();
        if (partNumbers.Distinct().Count() != partNumbers.Count)
            errors.Add("Duplicate part numbers detected.");

        if (partNumbers.Count != totalParts || partNumbers.First() != 1 || partNumbers.Last() != totalParts)
            errors.Add($"Expected part numbers 1..{totalParts}.");

        foreach (var part in storedParts)
        {
            if (part.SizeBytes > maxPartSizeBytes)
                errors.Add($"Part {part.PartNumber} exceeds size limit ({part.SizeBytes} bytes).");
        }

        var coveredPages = storedParts
            .SelectMany(p => p.PageIndices)
            .OrderBy(i => i)
            .ToList();

        if (!expectedPages.SequenceEqual(coveredPages))
            errors.Add("Page coverage mismatch between source and output parts.");

        var postFingerprint = ContentFingerprint.FromPageMetrics([.. sourceDocument.Pages.Select(p => new PageFingerprintEntry(p.PageIndex, p.TextLength, p.ImageBytesSum))]);

        if (postFingerprint.Value != preSplitFingerprint.Value)
            errors.Add("Content fingerprint mismatch after split.");

        return errors.Count is 0
            ? OutputValidationResult.Success()
            : OutputValidationResult.Failure([.. errors]);
    }
}
