using FiapX.Application.DTOs;
using FiapX.Application.UseCases.Videos;
using FiapX.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FiapX.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de vídeos
/// </summary>
[ApiController]
[Route("api/videos")]
[Authorize]
[Produces("application/json")]
public class VideosController : ControllerBase
{
    private readonly UploadVideoUseCase _uploadUseCase;
    private readonly GetUserVideosUseCase _getUserVideosUseCase;
    private readonly GetVideoStatusUseCase _getVideoStatusUseCase;
    private readonly DownloadVideoUseCase _downloadUseCase;
    private readonly IValidator<UploadVideoRequest> _uploadValidator;

    public VideosController(
        UploadVideoUseCase uploadUseCase,
        GetUserVideosUseCase getUserVideosUseCase,
        GetVideoStatusUseCase getVideoStatusUseCase,
        DownloadVideoUseCase downloadUseCase,
        IValidator<UploadVideoRequest> uploadValidator)
    {
        _uploadUseCase = uploadUseCase;
        _getUserVideosUseCase = getUserVideosUseCase;
        _getVideoStatusUseCase = getVideoStatusUseCase;
        _downloadUseCase = downloadUseCase;
        _uploadValidator = uploadValidator;
    }

    /// <summary>
    /// Faz upload de um vídeo para processamento
    /// </summary>
    /// <param name="file">Arquivo de vídeo (máximo 2GB)</param>
    /// <returns>ID do vídeo e status do processamento</returns>
    /// <response code="201">Vídeo enviado com sucesso</response>
    /// <response code="400">Arquivo inválido</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();

        var request = new UploadVideoRequest
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            FileSize = file.Length,
            UserId = userId
        };

        var validationResult = await _uploadValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        var result = await _uploadUseCase.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Success = false, Message = result.Error });

        return Created("", new { Success = true, Data = result.Value });
    }

    /// <summary>
    /// Lista todos os vídeos do usuário autenticado
    /// </summary>
    /// <returns>Lista de vídeos do usuário</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListVideos(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();

        var result = await _getUserVideosUseCase.ExecuteAsync(userId, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Value });
    }

    /// <summary>
    /// Retorna o status detalhado de um vídeo específico
    /// </summary>
    /// <param name="videoId">ID do vídeo</param>
    /// <returns>Status detalhado do vídeo</returns>
    /// <response code="200">Status retornado com sucesso</response>
    /// <response code="404">Vídeo não encontrado</response>
    [HttpGet("{videoId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus([FromRoute] Guid videoId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();

        var result = await _getVideoStatusUseCase.ExecuteAsync(videoId, userId, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Value });
    }

    /// <summary>
    /// Faz download do ZIP com os frames extraídos do vídeo
    /// </summary>
    /// <param name="videoId">ID do vídeo</param>
    /// <returns>Arquivo ZIP com os frames</returns>
    /// <response code="200">Download iniciado com sucesso</response>
    /// <response code="400">Vídeo não disponível para download</response>
    /// <response code="404">Vídeo não encontrado</response>
    [HttpGet("{videoId:guid}/download")]
    [Produces("application/zip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download([FromRoute] Guid videoId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();

        var result = await _downloadUseCase.ExecuteAsync(videoId, userId, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Success = false, Message = result.Error });

        var download = result.Value!;

        return File(
            download.FileStream,
            "application/zip",
            download.FileName
        );
    }

    /// <summary>
    /// Extrai o UserId do token JWT atual
    /// </summary>
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null)
            throw new UnauthorizedAccessException("Token inválido.");

        return Guid.Parse(userIdClaim.Value);
    }
}