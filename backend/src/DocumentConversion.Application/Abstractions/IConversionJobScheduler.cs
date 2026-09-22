using DocumentConversion.Domain.Jobs.ValueObjects;

namespace DocumentConversion.Application.Abstractions;

public interface IConversionJobScheduler
{
    void EnqueueProcessJob(JobId jobId);
}
