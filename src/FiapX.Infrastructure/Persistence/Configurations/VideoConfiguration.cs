using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapX.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.Property(v => v.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.FileSizeBytes)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(VideoStatus.Uploaded);

        builder.Property(v => v.UploadedAt)
            .IsRequired();

        builder.Property(v => v.ProcessedAt)
            .IsRequired(false);

        builder.Property(v => v.ZipPath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(v => v.FrameCount)
            .IsRequired(false);

        builder.Property(v => v.ErrorMessage)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(v => v.ProcessingDuration)
            .IsRequired(false);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(v => v.UserId)
            .HasDatabaseName("IX_Videos_UserId");

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("IX_Videos_Status");

        builder.HasIndex(v => v.UploadedAt)
            .HasDatabaseName("IX_Videos_UploadedAt");

        builder.HasIndex(v => v.ProcessedAt)
            .HasDatabaseName("IX_Videos_ProcessedAt");

        builder.HasIndex(v => new { v.UserId, v.Status })
            .HasDatabaseName("IX_Videos_UserId_Status");

        builder.HasOne(v => v.User)
            .WithMany(u => u.Videos)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}