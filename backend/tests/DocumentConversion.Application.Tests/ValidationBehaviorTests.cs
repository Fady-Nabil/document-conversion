using BuildingBlocks.Application.Behaviors;
using DocumentConversion.Application.Jobs.Commands;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocumentConversion.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Throws_WhenValidationFails()
    {
        var validator = new InlineValidator<SubmitConversionJobCommand>();
        validator.RuleFor(x => x.FileName).Must(_ => false).WithMessage("bad file");

        var behavior = new ValidationBehavior<SubmitConversionJobCommand, SubmitConversionJobResult>(
            new IValidator<SubmitConversionJobCommand>[] { validator });

        var act = () => behavior.Handle(
            new SubmitConversionJobCommand(Stream.Null, "x.pdf", "application/pdf", "Docx"),
            () => Task.FromResult(new SubmitConversionJobResult(Guid.NewGuid())),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
