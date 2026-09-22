using BuildingBlocks.Domain.Exceptions;
using DocumentConversion.Domain.Jobs.Enums;

namespace DocumentConversion.Domain.Jobs.Exceptions;

public class DomainException : ModuleDomainException<DomainErrorCode>
{
    protected DomainException(DomainErrorCode domainCode, string message, Exception? inner = null)
        : base(domainCode, message, inner)
    {
    }
}

public sealed class InvalidOrCorruptedInputException(string message, Exception? inner = null) : DomainException(DomainErrorCode.InvalidOrCorruptedInput, message, inner)
{
}

public sealed class ScannedDocumentNotSupportedException(string message) : DomainException(DomainErrorCode.ScannedDocumentNotSupported, message)
{
}

public sealed class UnsplittableContentExceedsLimitException(string message) : DomainException(DomainErrorCode.UnsplittableContentExceedsLimit, message)
{
}

public sealed class EmptyDocumentException(string message) : DomainException(DomainErrorCode.EmptyDocument, message)
{
}

public sealed class UnsupportedOutputFormatException(string message) : DomainException(DomainErrorCode.UnsupportedOutputFormat, message)
{
}
