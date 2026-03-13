using FiapX.Application.DTOs;
using FiapX.Shared.Constants;
using FluentValidation;

namespace FiapX.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MinimumLength(AuthConstants.MinNameLength)
            .WithMessage($"Nome deve ter pelo menos {AuthConstants.MinNameLength} caracteres.")
            .MaximumLength(AuthConstants.MaxNameLength)
            .WithMessage($"Nome pode ter no máximo {AuthConstants.MaxNameLength} caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(AuthConstants.MinPasswordLength)
            .WithMessage($"Senha deve ter pelo menos {AuthConstants.MinPasswordLength} caracteres.")
            .MaximumLength(AuthConstants.MaxPasswordLength)
            .WithMessage($"Senha pode ter no máximo {AuthConstants.MaxPasswordLength} caracteres.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirmação de senha é obrigatória.")
            .Equal(x => x.Password)
            .WithMessage("Senhas não coincidem.");
    }
}