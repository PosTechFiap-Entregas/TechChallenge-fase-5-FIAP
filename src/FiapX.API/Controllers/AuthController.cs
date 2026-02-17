using FiapX.Application.DTOs;
using FiapX.Application.UseCases.Auth;
using FiapX.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FiapX.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de autenticação
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly IValidator<RegisterUserRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        RegisterUserUseCase registerUseCase,
        LoginUseCase loginUseCase,
        IValidator<RegisterUserRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _registerUseCase = registerUseCase;
        _loginUseCase = loginUseCase;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    /// <param name="request">Dados do novo usuário</param>
    /// <returns>Token JWT do usuário registrado</returns>
    /// <response code="201">Usuário registrado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="409">Email já está em uso</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        // Validação
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        // Executar use case
        var result = await _registerUseCase.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return Conflict(new { Success = false, Message = result.Error });

        return Created("", new { Success = true, Data = result.Value });
    }

    /// <summary>
    /// Autentica um usuário existente
    /// </summary>
    /// <param name="request">Email e senha do usuário</param>
    /// <returns>Token JWT do usuário autenticado</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Email ou senha inválidos</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // Validação
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        // Executar use case
        var result = await _loginUseCase.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Value });
    }
}