using Microsoft.EntityFrameworkCore;
using JobRunner.Domain.Entities;

namespace JobRunner.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobRun> JobRuns => Set<JobRun>();
    public DbSet<JobAttempt> JobAttempts => Set<JobAttempt>();
    public DbSet<ExternalApiSyncResult> ExternalApiSyncResults => Set<ExternalApiSyncResult>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.JobType)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasMany(e => e.JobRuns)
                .WithOne(e => e.Job)
                .HasForeignKey(e => e.JobId);

        });

        modelBuilder.Entity<JobRun>(entity =>
        {
            entity.HasKey(e => e.Id);

            //entity.Property(e => e.RowVersion)
            //    .IsConcurrencyToken();

            entity.Property(e => e.Version).IsConcurrencyToken();

            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(128);

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.HasMany(e => e.JobAttempts)
                .WithOne(e => e.JobRun)
                .HasForeignKey(e => e.JobRunId);

            entity.HasOne(e => e.ExternalApiSyncResult)
                .WithOne(e => e.JobRun)
                .HasForeignKey<ExternalApiSyncResult>(e => e.JobRunId);

            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique();

            entity.HasIndex(e => e.NextVisibleUtc);

            entity.HasIndex(e => e.Status);

            entity.HasIndex(e => new { e.Status, e.NextVisibleUtc, e.CreatedAtUtc });
        });

        modelBuilder.Entity<JobAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ExternalApiSyncResult>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
        });
    }
}
