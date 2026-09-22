namespace DocumentConversion.Application.Dtos;

public sealed record JobArtifactDownloadDto(string FileName, string ContentType, Stream Content);
