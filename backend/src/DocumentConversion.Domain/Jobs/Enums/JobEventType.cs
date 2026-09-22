namespace DocumentConversion.Domain.Jobs.Enums;

public enum JobEventType
{
    Submitted = 1,
    Started = 2,
    ConversionSucceeded = 3,
    SplitIntoParts = 4,
    ValidationPassed = 5,
    ValidationFailed = 6,
    Completed = 7,
    Failed = 8,
    NeedsReview = 9
}
