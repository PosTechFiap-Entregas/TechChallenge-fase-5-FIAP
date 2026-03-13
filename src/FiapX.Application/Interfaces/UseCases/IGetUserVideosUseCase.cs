using FiapX.Application.DTOs;
using FiapX.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapX.Application.Interfaces.UseCases
{
    public interface IGetUserVideosUseCase
    {
        Task<Result<IEnumerable<VideoListResponse>>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}