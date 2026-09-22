using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;

namespace DocumentConversion.Application.Jobs.Commands;

public sealed class SubmitConversionJobCommandHandler : IRequestHandler<SubmitConversionJobCommand, SubmitConversionJobResult>
{
    private readonly IConversionJobRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IConversionJobScheduler _scheduler;

    public SubmitConversionJobCommandHandler(
        IConversionJobRepository repository,
        IFileStorage fileStorage,
        IConversionJobScheduler scheduler)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _scheduler = scheduler;
    }

    public async Task<SubmitConversionJobResult> Handle(SubmitConversionJobCommand request, CancellationToken cancellationToken)
    {
        await using (request.FileStream)
        {
            var format = OutputFormat.Parse(request.OutputFormat);
            var jobId = JobId.New();

            var relativePath = await _fileStorage.SaveSourceAsync(
                jobId.Value,
                request.FileName,
                request.FileStream,
                cancellationToken);

            var job = ConversionJob.Submit(jobId, format, request.FileName, relativePath);
            await _repository.AddAsync(job, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _scheduler.EnqueueProcessJob(jobId);

            return new SubmitConversionJobResult(jobId.Value);
        }
    }
}
