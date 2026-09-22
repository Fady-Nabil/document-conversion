using DocumentConversion.Application.Dtos;
using DocumentConversion.Application.Mapping;
using DocumentConversion.Domain.Jobs.Repositories;
using MediatR;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed class ListConversionJobsQueryHandler : IRequestHandler<ListConversionJobsQuery, PagedJobsDto>
{
    private readonly IConversionJobRepository _repository;

    public ListConversionJobsQueryHandler(IConversionJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedJobsDto> Handle(ListConversionJobsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var jobs = await _repository.ListAsync(skip, request.PageSize + 1, cancellationToken);
        var hasMore = jobs.Count > request.PageSize;
        var pageItems = jobs.Take(request.PageSize).Select(JobMapper.ToSummary).ToList();
        return new PagedJobsDto(pageItems, pageItems.Count + (hasMore ? 1 : 0));
    }
}
