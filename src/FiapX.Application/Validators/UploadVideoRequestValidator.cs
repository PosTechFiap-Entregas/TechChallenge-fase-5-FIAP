using FiapX.Application.DTOs;
using FiapX.Shared.Constants;
using FluentValidation;

namespace FiapX.Application.Validators;

/// <summary>
/// Validador para upload de vídeo
/// </summary>
public class UploadVideoRequestValidator : AbstractValidator<UploadVideoRequest>
{
    public UploadVideoRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("Nome do arquivo é obrigatório.")
            .Must(HasValidExtension)
            .WithMessage("Formato de arquivo não suportado. Use: mp4, avi, mov, mkv, wmv, flv, webm, m4v.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("Arquivo não pode estar vazio.")
            .LessThanOrEqualTo(VideoConstants.MaxFileSizeBytes)
            .WithMessage($"Tamanho máximo permitido é {VideoConstants.MaxFileSizeMB}MB (2GB).");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("Stream do arquivo é obrigatório.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório.");
    }

    private static bool HasValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return VideoConstants.AllowedExtensions.Contains(extension);
    }
}