using MediatR;

namespace DocumentConversion.Application.Jobs.Commands;

public sealed record SubmitConversionJobCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    string OutputFormat) : IRequest<SubmitConversionJobResult>;

public sealed record SubmitConversionJobResult(Guid JobId);
