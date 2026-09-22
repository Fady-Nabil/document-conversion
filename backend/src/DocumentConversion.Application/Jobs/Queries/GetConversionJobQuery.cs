using DocumentConversion.Application.Dtos;
using MediatR;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed record GetConversionJobQuery(Guid JobId) : IRequest<JobDetailDto>;
