using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Application.Options;
using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Entities;
using DocumentConversion.Domain.Jobs.Exceptions;
using DocumentConversion.Domain.Jobs.Models;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.Services;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocumentConversion.Application.Jobs.Commands;

public sealed class ProcessConversionJobCommandHandler : IRequestHandler<ProcessConversionJobCommand, Unit>
{
    private readonly IConversionJobRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IPdfAnalyzer _pdfAnalyzer;
    private readonly IDocumentConverter _documentConverter;
    private readonly ConversionOptions _options;

    public ProcessConversionJobCommandHandler(
        IConversionJobRepository repository,
        IFileStorage fileStorage,
        IPdfAnalyzer pdfAnalyzer,
        IDocumentConverter documentConverter,
        IOptions<ConversionOptions> options)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _pdfAnalyzer = pdfAnalyzer;
        _documentConverter = documentConverter;
        _options = options.Value;
    }

    public async Task<Unit> Handle(ProcessConversionJobCommand request, CancellationToken cancellationToken)
    {
        var jobId = JobId.From(request.JobId);
        var job = await _repository.GetByIdForProcessingAsync(jobId, cancellationToken);
        if (job is null)
            return Unit.Value;

        if (job.IsTerminal)
            return Unit.Value;

        await _repository.MarkProcessingAsync(jobId, cancellationToken);
        _repository.DetachJobGraph(jobId);
        job = await _repository.GetByIdForProcessingAsync(jobId, cancellationToken);
        if (job is null || job.IsTerminal)
            return Unit.Value;

        await using var pdfStream = await _fileStorage.OpenReadAsync(job.SourceRelativePath, cancellationToken);
        using var memoryCopy = new MemoryStream();
        await pdfStream.CopyToAsync(memoryCopy, cancellationToken);
        var pdfBytes = memoryCopy.ToArray();

        var analysis = await _pdfAnalyzer.AnalyzeAsync(new MemoryStream(pdfBytes), cancellationToken);

        if (analysis.TotalExtractableCharacters == 0)
            throw new ScannedDocumentNotSupportedException("Document has no extractable text (scanned/image-only PDF).");

        var document = await _documentConverter.ConvertPdfToDocumentModelAsync(new MemoryStream(pdfBytes), cancellationToken);
        if (document.IsEmpty)
            throw new EmptyDocumentException("Converted document is empty.");

        var fingerprint = ContentFingerprint.FromPageMetrics(
            document.Pages.Select(p => new PageFingerprintEntry(p.PageIndex, p.TextLength, p.ImageBytesSum)).ToList());

        job.RecordConversionSucceeded(fingerprint);

        var partPlans = DocumentSplittingService.PlanParts(document, _options.MaxOutputPartSizeBytes);
        if (partPlans.Count > 1)
            job.RecordSplitIntoParts(partPlans.Count);

        var storedParts = new List<StoredPartInfo>();
        var artifacts = new List<OutputArtifact>();

        foreach (var plan in partPlans)
        {
            plan.DocxContent = _documentConverter.BuildPartDocx(document, plan.PageIndices);
            var sequence = new PartSequence(plan.PartNumber, plan.TotalParts);
            var fileName = $"{job.Id}_part_{plan.PartNumber}_of_{plan.TotalParts}.docx";

            await using var docxStream = new MemoryStream(plan.DocxContent);
            var relativePath = await _fileStorage.SaveArtifactAsync(job.Id, fileName, docxStream, cancellationToken);

            artifacts.Add(OutputArtifact.Create(job.Id, sequence, fileName, relativePath, plan.DocxContent.LongLength));
            storedParts.Add(new StoredPartInfo
            {
                PartNumber = plan.PartNumber,
                TotalParts = plan.TotalParts,
                PageIndices = plan.PageIndices,
                SizeBytes = plan.DocxContent.LongLength
            });
        }

        job.AttachArtifacts(artifacts);

        var validation = OutputValidationService.Validate(
            document,
            fingerprint,
            storedParts,
            _options.MaxOutputPartSizeBytes);

        if (!validation.IsValid)
        {
            job.MarkNeedsReview(string.Join("; ", validation.Errors));
            await _repository.MarkNeedsReviewAsync(job, cancellationToken);
        }
        else
        {
            job.Complete();
            await _repository.CompleteAsync(job, cancellationToken);
        }

        return Unit.Value;
    }
}
