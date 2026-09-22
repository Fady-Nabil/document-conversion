namespace DocumentConversion.Domain.Jobs.Models;

public sealed class OutputValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static OutputValidationResult Success() => new() { IsValid = true };

    public static OutputValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };
}
