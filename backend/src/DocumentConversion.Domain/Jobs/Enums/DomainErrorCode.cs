namespace DocumentConversion.Domain.Jobs.Enums;

public enum DomainErrorCode
{
    InvalidOrCorruptedInput = 1,
    UnsupportedOutputFormat = 2,
    ScannedDocumentNotSupported = 3,
    UnsplittableContentExceedsLimit = 4,
    EmptyDocument = 5,
    ValidationFailed = 6,
    UnexpectedError = 99
}
