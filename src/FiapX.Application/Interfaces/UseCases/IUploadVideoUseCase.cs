using FiapX.Application.DTOs;
using FiapX.Shared.Results;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface IUploadVideoUseCase
    {
        Task<Result<UploadVideoResponse>> ExecuteAsync(UploadVideoRequest request, CancellationToken cancellationToken = default);
    }
}