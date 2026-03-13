using FiapX.Application.DTOs;
using FiapX.Application.Interfaces.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FiapX.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IRegisterUserUseCase _registerUseCase;
    private readonly ILoginUseCase _loginUseCase;
    private readonly IValidator<RegisterUserRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IRegisterUserUseCase registerUseCase,
        ILoginUseCase loginUseCase,
        IValidator<RegisterUserRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _registerUseCase = registerUseCase;
        _loginUseCase = loginUseCase;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        var result = await _registerUseCase.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return Conflict(new { Success = false, Message = result.Error });

        return Created("", new { Success = true, Data = result.Value });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Success = false, Errors = errors });
        }

        var result = await _loginUseCase.ExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Value });
    }
}