using MediatR;

namespace DocumentConversion.Application.Jobs.Commands;

public sealed record ProcessConversionJobCommand(Guid JobId) : IRequest<Unit>;
