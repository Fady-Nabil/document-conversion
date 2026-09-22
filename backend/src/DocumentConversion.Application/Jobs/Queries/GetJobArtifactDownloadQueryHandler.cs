using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Dtos;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;
using Shared.Exceptions;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed class GetJobArtifactDownloadQueryHandler : IRequestHandler<GetJobArtifactDownloadQuery, JobArtifactDownloadDto>
{
    private readonly IConversionJobRepository _repository;
    private readonly IFileStorage _fileStorage;

    public GetJobArtifactDownloadQueryHandler(
        IConversionJobRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<JobArtifactDownloadDto> Handle(GetJobArtifactDownloadQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(JobId.From(request.JobId), cancellationToken);
        if (job is null)
            throw new NotFoundException("Conversion job not found.");

        var artifact = job.Artifacts.FirstOrDefault(a => a.PartNumber == request.PartNumber);
        if (artifact is null)
            throw new NotFoundException("Artifact not found for this job.");

        var absolute = _fileStorage.GetAbsolutePath(artifact.RelativePath);
        if (!File.Exists(absolute))
            throw new NotFoundException("Artifact file is not available.");

        var content = await _fileStorage.OpenReadAsync(artifact.RelativePath, cancellationToken);
        return new JobArtifactDownloadDto(artifact.FileName, artifact.ContentType, content);
    }
}
