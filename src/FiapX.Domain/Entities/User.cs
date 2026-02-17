namespace FiapX.Domain.Entities;

/// <summary>
/// Entidade de usuário do sistema
/// </summary>
public class User : BaseEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ICollection<Video> Videos { get; private set; }

    private User() 
    { 
        Email = string.Empty;
        PasswordHash = string.Empty;
        Name = string.Empty;
        Videos = new List<Video>();
    }

    public User(string email, string passwordHash, string name) : base()
    {
        ValidateEmail(email);
        ValidateName(name);

        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        Name = name.Trim();
        IsActive = true;
        Videos = new List<Video>();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void UpdateName(string newName)
    {
        ValidateName(newName);
        Name = newName.Trim();
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 256)
            throw new ArgumentException("Email too long", nameof(email));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name too long", nameof(name));
    }
}
