using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.Models;
using DocumentConversion.Domain.Jobs.Services;
using DocumentConversion.Domain.Jobs.ValueObjects;
using FluentAssertions;

namespace DocumentConversion.Domain.Tests;

public class OutputValidationServiceTests
{
    [Fact]
    public void Validate_WhenConsistent_ReturnsSuccess()
    {
        var doc = new DocumentModel
        {
            Pages = new List<PageModel>
            {
                new() { PageIndex = 1, Text = "hello", SerializedDocx = new byte[10] }
            }
        };
        var fp = ContentFingerprint.FromPageMetrics(new[] { new PageFingerprintEntry(1, 5, 0) });
        var parts = new List<StoredPartInfo>
        {
            new() { PartNumber = 1, TotalParts = 1, PageIndices = new[] { 1 }, SizeBytes = 10 }
        };

        var result = OutputValidationService.Validate(doc, fp, parts, 2_097_152);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPageCoverageMismatch_ReturnsFailure()
    {
        var doc = new DocumentModel
        {
            Pages = new List<PageModel>
            {
                new() { PageIndex = 1, Text = "a", SerializedDocx = new byte[10] },
                new() { PageIndex = 2, Text = "b", SerializedDocx = new byte[10] }
            }
        };
        var fp = ContentFingerprint.FromPageMetrics(new[]
        {
            new PageFingerprintEntry(1, 1, 0),
            new PageFingerprintEntry(2, 1, 0)
        });
        var parts = new List<StoredPartInfo>
        {
            new() { PartNumber = 1, TotalParts = 1, PageIndices = new[] { 1 }, SizeBytes = 10 }
        };

        var result = OutputValidationService.Validate(doc, fp, parts, 2_097_152);
        result.IsValid.Should().BeFalse();
    }
}

public class ConversionJobAggregateTests
{
    [Fact]
    public void Complete_WithoutArtifacts_Throws()
    {
        var job = ConversionJob.Submit(JobId.New(), OutputFormat.Docx, "a.pdf", "path");
        var act = () => job.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkNeedsReview_SetsStatusAndEvents()
    {
        var job = ConversionJob.Submit(JobId.New(), OutputFormat.Docx, "a.pdf", "path");
        job.MarkNeedsReview("validation failed");
        job.Status.Should().Be(JobStatus.NeedsReview);
        job.Events.Should().Contain(e => e.EventType == JobEventType.NeedsReview);
    }
}
