namespace DocumentConversion.Application.Options;

public sealed class ConversionOptions
{
    public const string SectionName = "Conversion";

    public long MaxOutputPartSizeBytes { get; set; } = 2_097_152;
    public long MaxUploadSizeBytes { get; set; } = 50_000_000;
}
