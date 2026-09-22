using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Domain.Jobs.Repositories;

public interface IConversionJobRepository
{
    Task AddAsync(ConversionJob job, CancellationToken cancellationToken = default);
    Task<ConversionJob?> GetByIdAsync(JobId id, CancellationToken cancellationToken = default);
    Task<ConversionJob?> GetByIdForProcessingAsync(JobId id, CancellationToken cancellationToken = default);
    Task MarkProcessingAsync(JobId id, CancellationToken cancellationToken = default);
    Task FailJobAsync(JobId id, DomainErrorCode code, string message, CancellationToken cancellationToken = default);
    Task CompleteAsync(ConversionJob job, CancellationToken cancellationToken = default);
    Task MarkNeedsReviewAsync(ConversionJob job, CancellationToken cancellationToken = default);
    void DetachJobGraph(JobId id);
    Task<IReadOnlyList<ConversionJob>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
