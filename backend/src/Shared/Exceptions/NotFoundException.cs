namespace Shared.Exceptions;

public sealed class NotFoundException(string message) 
    : BusinessException("NOT_FOUND", message)
{
}
