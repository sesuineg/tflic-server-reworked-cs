namespace Server.Controllers.Version1.DTO;

public record AuthorizeRequestDto
{
    public AuthorizeRequestDto(string login, string passwordHash)
    {
        Login = login;
        PasswordHash = passwordHash;
    }

    public string Login { get; }
    public string PasswordHash { get; }
}