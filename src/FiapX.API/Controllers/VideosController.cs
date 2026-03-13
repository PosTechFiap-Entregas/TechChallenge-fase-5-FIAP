using FiapX.Application.DTOs;
using FiapX.Application.Interfaces.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FiapX.API.Controllers;

[ApiController]
[Route("api/videos")]
[Authorize]
[Produces("application/json")]
public class VideosController : ControllerBase
{
    private readonly IUploadVideoUseCase _uploadUseCase;
    private readonly IGetUserVideosUseCase _getUserVideosUseCase;
    private readonly IGetVideoStatusUseCase _getVideoStatusUseCase;
    private readonly IDownloadVideoUseCase _downloadUseCase;
    private readonly IValidator<UploadVideoRequest> _uploadValidator;

    public VideosController(
        IUploadVideoUseCase uploadUseCase,
        IGetUserVideosUseCase getUserVideosUseCase,
        IGetVideoStatusUseCase getVideoStatusUseCase,
        IDownloadVideoUseCase downloadUseCase,
        IValidator<UploadVideoRequest> uploadValidator)
    {
        _uploadUseCase = uploadUseCase;
        _getUserVideosUseCase = getUserVideosUseCase;
        _getVideoStatusUseCase = getVideoStatusUseCase;
        _downloadUseCase = downloadUseCase;
        _uploadValidator = uploadValidator;
    }

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

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null)
            throw new UnauthorizedAccessException("Token inválido.");

        return Guid.Parse(userIdClaim.Value);
    }
}