using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Entities;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.ValueObjects;
using Microsoft.EntityFrameworkCore;
using DomainJobEvent = DocumentConversion.Domain.Jobs.Entities.JobEvent;

namespace DocumentConversion.Infrastructure.Persistence;

internal sealed class ConversionJobRepository(AppDbContext db) : IConversionJobRepository
{
    private readonly AppDbContext _db = db;

    public Task AddAsync(ConversionJob job, CancellationToken cancellationToken = default) =>
        _db.Jobs.AddAsync(job, cancellationToken).AsTask();

    public Task<ConversionJob?> GetByIdAsync(JobId id, CancellationToken cancellationToken = default) =>
        _db.Jobs
            .AsSplitQuery()
            .Include(j => j.Events)
            .Include(j => j.Artifacts)
            .FirstOrDefaultAsync(j => j.Id == id.Value, cancellationToken);

    public async Task<ConversionJob?> GetByIdForProcessingAsync(JobId id, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id.Value, cancellationToken);
        if (job is null)
            return null;

        // Do not load existing events into the change tracker (avoids spurious UPDATE on timeline rows).
        _db.Entry(job).Collection(j => j.Events).IsLoaded = true;

        await _db.Entry(job).Collection(j => j.Artifacts).LoadAsync(cancellationToken);
        foreach (var artifact in job.Artifacts)
            _db.Entry(artifact).State = EntityState.Unchanged;

        return job;
    }

    public async Task MarkProcessingAsync(JobId id, CancellationToken cancellationToken = default)
    {
        var updated = await _db.Jobs
            .Where(j => j.Id == id.Value && j.Status == JobStatus.Submitted)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(j => j.Status, JobStatus.Processing),
                cancellationToken);

        if (updated == 0)
            return;

        _db.Set<JobEvent>().Add(DomainJobEvent.Create(id.Value, JobEventType.Started, "Processing started."));
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task FailJobAsync(JobId id, DomainErrorCode code, string message, CancellationToken cancellationToken = default)
    {
        var completedAt = DateTimeOffset.UtcNow;
        var updated = await _db.Jobs
            .Where(j => j.Id == id.Value
                        && j.Status != JobStatus.Completed
                        && j.Status != JobStatus.Failed
                        && j.Status != JobStatus.NeedsReview)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.ErrorCode, code)
                    .SetProperty(j => j.ErrorMessage, message)
                    .SetProperty(j => j.CompletedAt, completedAt),
                cancellationToken);

        if (updated == 0)
            return;

        _db.Set<JobEvent>().Add(DomainJobEvent.Create(id.Value, JobEventType.Failed, $"{code}: {message}"));
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(ConversionJob job, CancellationToken cancellationToken = default)
    {
        var jobId = JobId.From(job.Id);
        DetachJobGraph(jobId);

        var completedAt = DateTimeOffset.UtcNow;
        await _db.Jobs
            .Where(j => j.Id == job.Id && j.Status == JobStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(j => j.Status, JobStatus.Completed)
                    .SetProperty(j => j.CompletedAt, completedAt)
                    .SetProperty(j => j.ContentFingerprint, job.ContentFingerprint)
                    .SetProperty(j => j.ErrorCode, (DomainErrorCode?)null)
                    .SetProperty(j => j.ErrorMessage, (string?)null),
                cancellationToken);

        foreach (var jobEvent in job.Events)
            _db.Set<JobEvent>().Add(jobEvent);

        foreach (var artifact in job.Artifacts)
            _db.Set<OutputArtifact>().Add(artifact);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkNeedsReviewAsync(ConversionJob job, CancellationToken cancellationToken = default)
    {
        var jobId = JobId.From(job.Id);
        DetachJobGraph(jobId);

        var completedAt = DateTimeOffset.UtcNow;
        await _db.Jobs
            .Where(j => j.Id == job.Id && j.Status == JobStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(j => j.Status, JobStatus.NeedsReview)
                    .SetProperty(j => j.CompletedAt, completedAt)
                    .SetProperty(j => j.ErrorCode, DomainErrorCode.ValidationFailed)
                    .SetProperty(j => j.ErrorMessage, job.ErrorMessage),
                cancellationToken);

        foreach (var jobEvent in job.Events)
            _db.Set<JobEvent>().Add(jobEvent);

        foreach (var artifact in job.Artifacts)
            _db.Set<OutputArtifact>().Add(artifact);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public void DetachJobGraph(JobId id)
    {
        foreach (var entry in _db.ChangeTracker.Entries<JobEvent>().Where(e => e.Entity.JobId == id.Value).ToList())
            entry.State = EntityState.Detached;

        foreach (var entry in _db.ChangeTracker.Entries<OutputArtifact>().Where(e => e.Entity.JobId == id.Value).ToList())
            entry.State = EntityState.Detached;

        var trackedJob = _db.Jobs.Local.FirstOrDefault(j => j.Id == id.Value);
        if (trackedJob is not null)
            _db.Entry(trackedJob).State = EntityState.Detached;
    }

    public async Task<IReadOnlyList<ConversionJob>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        // SQLite provider cannot ORDER BY DateTimeOffset; sort in memory (demo-scale job count).
        var jobs = await _db.Jobs.ToListAsync(cancellationToken);
        return jobs
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
