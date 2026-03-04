using FiapX.Application.DTOs;
using FiapX.Application.Events;
using FiapX.Application.Interfaces.UseCases;
using FiapX.Domain.Entities;
using FiapX.Domain.Interfaces;
using FiapX.Shared.Results;

namespace FiapX.Application.UseCases.Videos;

/// <summary>
/// Use Case para upload de vídeo
/// </summary>
public class UploadVideoUseCase : IUploadVideoUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IMessagePublisher _messagePublisher;

    public UploadVideoUseCase(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IMessagePublisher messagePublisher)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<UploadVideoResponse>> ExecuteAsync(
        UploadVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verificar se o usuário existe
        var user = await _unitOfWork.Users
            .GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UploadVideoResponse>("Usuário não encontrado.");

        // Salvar arquivo no storage
        var storagePath = await _storageService
            .SaveVideoAsync(request.FileStream, request.FileName, cancellationToken);

        // Criar entidade de vídeo
        var video = new Video(
            userId: request.UserId,
            originalFileName: request.FileName,
            storagePath: storagePath,
            fileSizeBytes: request.FileSize
        );

        // Marcar como na fila
        video.MarkAsQueued();

        // Persistir no banco
        await _unitOfWork.Videos.AddAsync(video, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publicar evento na fila
        await _messagePublisher.PublishAsync(new VideoUploadedEvent
        {
            VideoId = video.Id,
            UserId = video.UserId,
            StoragePath = video.StoragePath,
            OriginalFileName = video.OriginalFileName,
            FileSizeBytes = video.FileSizeBytes,
            UploadedAt = video.UploadedAt
        }, cancellationToken);

        var response = new UploadVideoResponse
        {
            VideoId = video.Id,
            OriginalFileName = video.OriginalFileName,
            Status = video.Status.ToString(),
            Message = "Vídeo enviado com sucesso e está sendo processado."
        };

        return Result.Success(response);
    }
}