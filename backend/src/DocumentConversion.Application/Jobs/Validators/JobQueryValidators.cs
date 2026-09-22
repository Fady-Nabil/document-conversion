using DocumentConversion.Application.Jobs.Queries;
using DocumentConversion.Application.Jobs.Commands;
using FluentValidation;

namespace DocumentConversion.Application.Jobs.Validators;

public sealed class GetConversionJobQueryValidator : AbstractValidator<GetConversionJobQuery>
{
    public GetConversionJobQueryValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
    }
}

public sealed class ListConversionJobsQueryValidator : AbstractValidator<ListConversionJobsQuery>
{
    public ListConversionJobsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetJobArtifactDownloadQueryValidator : AbstractValidator<GetJobArtifactDownloadQuery>
{
    public GetJobArtifactDownloadQueryValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.PartNumber).GreaterThanOrEqualTo(1);
    }
}
