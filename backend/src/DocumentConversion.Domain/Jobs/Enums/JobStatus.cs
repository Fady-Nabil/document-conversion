namespace DocumentConversion.Domain.Jobs.Enums;

public enum JobStatus
{
    Submitted = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    NeedsReview = 5
}
