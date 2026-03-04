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
    /// Interface para o caso de uso de listagem de vídeos do usuário
    /// </summary>
    public interface IGetUserVideosUseCase
    {
        Task<Result<IEnumerable<VideoListResponse>>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}