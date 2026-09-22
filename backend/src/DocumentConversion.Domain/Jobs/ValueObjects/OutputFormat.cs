namespace DocumentConversion.Domain.Jobs.ValueObjects;

public readonly record struct OutputFormat(string Value)
{
    public static OutputFormat Docx { get; } = new("Docx");

    public static OutputFormat Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Output format is required.", nameof(value));

        if (value.Equals("Docx", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("docx", StringComparison.OrdinalIgnoreCase))
            return Docx;

        throw new ArgumentException($"Unsupported output format: {value}", nameof(value));
    }

    public static bool TryParse(string? value, out OutputFormat format)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            format = default;
            return false;
        }

        if (value.Equals("Docx", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("docx", StringComparison.OrdinalIgnoreCase))
        {
            format = Docx;
            return true;
        }

        format = default;
        return false;
    }
}
