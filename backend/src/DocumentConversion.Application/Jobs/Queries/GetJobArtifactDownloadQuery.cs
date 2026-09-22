using DocumentConversion.Application.Dtos;
using MediatR;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed record GetJobArtifactDownloadQuery(Guid JobId, int PartNumber) : IRequest<JobArtifactDownloadDto>;
