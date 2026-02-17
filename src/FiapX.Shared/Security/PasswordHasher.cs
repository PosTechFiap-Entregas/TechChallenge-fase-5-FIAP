using BCrypt.Net;

namespace FiapX.Shared.Security;

/// <summary>
/// Serviço para hash e verificação de senhas usando BCrypt
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// Gera um hash da senha usando BCrypt
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash
    /// </summary>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}