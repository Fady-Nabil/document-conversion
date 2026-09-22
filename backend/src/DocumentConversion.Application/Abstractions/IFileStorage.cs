namespace DocumentConversion.Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveSourceAsync(Guid jobId, string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<string> SaveArtifactAsync(Guid jobId, string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}
