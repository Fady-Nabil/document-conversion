using DocumentConversion.Domain.Jobs.Aggregates;
using DocumentConversion.Domain.Jobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentConversion.Infrastructure.Persistence.Configurations;

internal sealed class ConversionJobConfiguration : IEntityTypeConfiguration<ConversionJob>
{
    public void Configure(EntityTypeBuilder<ConversionJob> builder)
    {
        builder.ToTable("ConversionJobs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.OutputFormat).HasMaxLength(32);
        builder.Property(x => x.SourceFileName).HasMaxLength(512);
        builder.Property(x => x.SourceRelativePath).HasMaxLength(1024);
        builder.Property(x => x.ErrorCode).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.ContentFingerprint).HasMaxLength(8000);

        builder.HasMany(j => j.Events)
            .WithOne()
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.Artifacts)
            .WithOne()
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(j => j.Events).HasField("_events");
        builder.Navigation(j => j.Artifacts).HasField("_artifacts");
    }
}

internal sealed class JobEventConfiguration : IEntityTypeConfiguration<JobEvent>
{
    public void Configure(EntityTypeBuilder<JobEvent> builder)
    {
        builder.ToTable("JobEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.Message).HasMaxLength(4000);
    }
}

internal sealed class OutputArtifactConfiguration : IEntityTypeConfiguration<OutputArtifact>
{
    public void Configure(EntityTypeBuilder<OutputArtifact> builder)
    {
        builder.ToTable("OutputArtifacts");
        builder.HasKey(x => x.Id);
        builder.Property(a => a.FileName).HasMaxLength(512);
        builder.Property(a => a.RelativePath).HasMaxLength(1024);
        builder.Property(a => a.ContentType).HasMaxLength(256);
    }
}
