using FiapX.Application.DTOs;
using FiapX.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapX.Application.Interfaces.UseCases
{
    /// <summary>
    /// Interface para o caso de uso de upload de vídeo
    /// </summary>
    public interface IUploadVideoUseCase
    {
        Task<Result<UploadVideoResponse>> ExecuteAsync(UploadVideoRequest request, CancellationToken cancellationToken = default);
    }
}