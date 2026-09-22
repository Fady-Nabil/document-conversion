using Shared.Exceptions;

namespace BuildingBlocks.Domain.Exceptions;

public abstract class ModuleDomainException<TErrorCode>(TErrorCode domainCode, string message, Exception? inner = null) 
    : BusinessException(domainCode.ToString(), message, inner)
    where TErrorCode : struct, Enum
{
    public TErrorCode DomainCode { get; } = domainCode;
}
