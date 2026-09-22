using DocumentConversion.Domain.Jobs.Exceptions;
using DocumentConversion.Domain.Jobs.Models;
using DocumentConversion.Domain.Jobs.Services;
using FluentAssertions;

namespace DocumentConversion.Domain.Tests;

public class DocumentSplittingServiceTests
{
    [Fact]
    public void PlanParts_WhenUnderLimit_ReturnsSinglePart()
    {
        var doc = CreateDocument(new[] { 1000L, 1500L });
        var parts = DocumentSplittingService.PlanParts(doc, 2_097_152);
        parts.Should().HaveCount(1);
        parts[0].TotalParts.Should().Be(1);
        parts[0].PageIndices.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void PlanParts_WhenOverLimit_ReturnsMultipleParts()
    {
        var doc = CreateDocument(new[] { 1_500_000L, 1_500_000L });
        var parts = DocumentSplittingService.PlanParts(doc, 2_097_152);
        parts.Should().HaveCount(2);
        parts[0].PartNumber.Should().Be(1);
        parts[1].PartNumber.Should().Be(2);
    }

    [Fact]
    public void PlanParts_WhenSinglePageExceedsLimit_Throws()
    {
        var doc = CreateDocument(new[] { 3_000_000L });
        var act = () => DocumentSplittingService.PlanParts(doc, 2_097_152);
        act.Should().Throw<UnsplittableContentExceedsLimitException>();
    }

    [Fact]
    public void PlanParts_WhenExactlyAtLimit_SinglePart()
    {
        var doc = CreateDocument(new[] { 2_097_152L });
        var parts = DocumentSplittingService.PlanParts(doc, 2_097_152);
        parts.Should().HaveCount(1);
    }

    private static DocumentModel CreateDocument(long[] pageSizes)
    {
        var pages = pageSizes.Select((size, i) => new PageModel
        {
            PageIndex = i + 1,
            Text = new string('x', (int)Math.Min(size, 100)),
            SerializedDocx = new byte[size]
        }).ToList();
        return new DocumentModel { Pages = pages };
    }
}
