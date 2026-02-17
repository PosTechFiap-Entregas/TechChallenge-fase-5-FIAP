using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapX.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Video
/// </summary>
public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        // Tabela
        builder.ToTable("Videos");

        // Chave primária
        builder.HasKey(v => v.Id);

        // Propriedades
        builder.Property(v => v.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Guid gerado pela aplicação

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
            .HasConversion<int>() // Armazena como int no banco
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

        // Índices
        builder.HasIndex(v => v.UserId)
            .HasDatabaseName("IX_Videos_UserId");

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("IX_Videos_Status");

        builder.HasIndex(v => v.UploadedAt)
            .HasDatabaseName("IX_Videos_UploadedAt");

        builder.HasIndex(v => v.ProcessedAt)
            .HasDatabaseName("IX_Videos_ProcessedAt");

        // Índice composto para queries comuns
        builder.HasIndex(v => new { v.UserId, v.Status })
            .HasDatabaseName("IX_Videos_UserId_Status");

        // Relacionamentos
        builder.HasOne(v => v.User)
            .WithMany(u => u.Videos)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}