using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Domain.Jobs.ValueObjects;
using FluentValidation;

namespace DocumentConversion.Application.Jobs.Validators;

public sealed class SubmitConversionJobCommandValidator : AbstractValidator<SubmitConversionJobCommand>
{
    public SubmitConversionJobCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .Must(s => s is not null && s.CanRead && s.Length > 0)
            .WithMessage("PDF file is required.");
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.OutputFormat).NotEmpty();
        RuleFor(x => x.FileName)
            .Must(n => n.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only PDF input files are supported.");
        RuleFor(x => x.ContentType)
            .Must(ct => string.IsNullOrEmpty(ct) || ct.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Content type must indicate PDF.");
        RuleFor(x => x.OutputFormat)
            .Must(f => OutputFormat.TryParse(f, out _))
            .WithMessage("Unsupported output format. Only Docx is supported.");
    }
}

public sealed class ProcessConversionJobCommandValidator : AbstractValidator<ProcessConversionJobCommand>
{
    public ProcessConversionJobCommandValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
    }
}
