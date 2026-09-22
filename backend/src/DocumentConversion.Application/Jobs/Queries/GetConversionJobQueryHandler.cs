using DocumentConversion.Application.Dtos;
using DocumentConversion.Application.Mapping;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;
using Shared.Exceptions;

namespace DocumentConversion.Application.Jobs.Queries;

public sealed class GetConversionJobQueryHandler : IRequestHandler<GetConversionJobQuery, JobDetailDto>
{
    private readonly IConversionJobRepository _repository;

    public GetConversionJobQueryHandler(IConversionJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<JobDetailDto> Handle(GetConversionJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(JobId.From(request.JobId), cancellationToken);
        if (job is null)
            throw new NotFoundException("Conversion job not found.");

        return JobMapper.ToDetail(job);
    }
}
