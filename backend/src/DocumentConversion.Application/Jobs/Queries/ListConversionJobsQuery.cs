using DocumentConversion.Application.Dtos;
using MediatR;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed record ListConversionJobsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedJobsDto>;
